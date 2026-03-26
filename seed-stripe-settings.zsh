#!/usr/bin/env zsh
set -euo pipefail

# seed-stripe-settings.zsh
#
# Reads key=value pairs from a file and upserts each setting via:
#   PUT /api/v1/admin/settings/{key}
#
# Usage:
#   chmod +x seed-stripe-settings.zsh
#   ./seed-stripe-settings.zsh stripe-settings.env
#
# Optional env overrides:
#   AUTH_URL=http://localhost:5201/api/v1/auth/login
#   SETTINGS_BASE_URL=http://localhost:5201/api/v1/admin/settings
#   LOGIN_EMAIL=aethon@localhost.com
#   LOGIN_PASSWORD='Aethon@Admin2026!'
#
# Example input file:
#   Stripe.WebhookSecret=whsec_xxx
#   Stripe.Price.VerificationStandard=price_abc
#   Stripe.Price.VerificationEnhanced=price_def
#   Stripe.Price.Sticky.Verified.24h=price_ghi
#   Stripe.Price.Sticky.Verified.7d=price_jkl

AUTH_URL="${AUTH_URL:-https://dev-api.app.aethonsoftware.com/api/v1/auth/login}"
SETTINGS_BASE_URL="${SETTINGS_BASE_URL:-https://dev-api.app.aethonsoftware.com/api/v1/admin/settings}"
LOGIN_EMAIL="${LOGIN_EMAIL:-aethon@localhost.com}"
LOGIN_PASSWORD="${LOGIN_PASSWORD:-Aethon@Admin2026!}"

SETTINGS_FILE="${1:-stripe-settings.env}"

if [[ ! -f "$SETTINGS_FILE" ]]; then
  echo "Settings file not found: $SETTINGS_FILE" >&2
  exit 1
fi

if ! command -v curl >/dev/null 2>&1; then
  echo "curl is required but not installed." >&2
  exit 1
fi

# Prefer jq for JSON escaping/parsing if available.
HAS_JQ=0
if command -v jq >/dev/null 2>&1; then
  HAS_JQ=1
fi

json_escape() {
  local raw="$1"

  if [[ "$HAS_JQ" -eq 1 ]]; then
    jq -Rn --arg v "$raw" '{ value: $v }'
  elif command -v python3 >/dev/null 2>&1; then
    python3 - <<PY
import json
print(json.dumps({"value": """$raw""" }))
PY
  else
    # Fallback: minimal escaping for quotes/backslashes/newlines.
    local escaped="${raw//\\/\\\\}"
    escaped="${escaped//\"/\\\"}"
    escaped="${escaped//$'\n'/\\n}"
    printf '{"value":"%s"}\n' "$escaped"
  fi
}

url_encode() {
  local raw="$1"

  if command -v python3 >/dev/null 2>&1; then
    RAW_VALUE="$raw" python3 - <<'PY'
import os
import urllib.parse
print(urllib.parse.quote(os.environ["RAW_VALUE"], safe=".-_~"))
PY
  else
    # Dots are fine, but this fallback is intentionally limited.
    # Strongly prefer python3 for correctness.
    local encoded="${raw// /%20}"
    encoded="${encoded//\//%2F}"
    printf '%s\n' "$encoded"
  fi
}

trim() {
  local s="$1"
  s="${s#"${s%%[![:space:]]*}"}"
  s="${s%"${s##*[![:space:]]}"}"
  printf '%s' "$s"
}

echo "Logging in at: $AUTH_URL"

login_payload=$(
  cat <<JSON
{
  "email": "$LOGIN_EMAIL",
  "password": "$LOGIN_PASSWORD"
}
JSON
)

login_response="$(
  curl --silent --show-error --fail \
    -X POST "$AUTH_URL" \
    -H "Content-Type: application/json" \
    -d "$login_payload"
)"

token=""
if [[ "$HAS_JQ" -eq 1 ]]; then
  token="$(printf '%s' "$login_response" | jq -r '.token // empty')"
else
  token="$(printf '%s' "$login_response" | sed -n 's/.*"token":"\([^"]*\)".*/\1/p')"
fi

if [[ -z "$token" ]]; then
  echo "Failed to extract token from login response." >&2
  echo "Raw response:" >&2
  printf '%s\n' "$login_response" >&2
  exit 1
fi

echo "Token extracted."
echo "Applying settings from: $SETTINGS_FILE"
echo

line_no=0
success_count=0
skip_count=0

while IFS= read -r line || [[ -n "$line" ]]; do
  line_no=$(( line_no + 1 ))

  # Ignore blank lines and comments
  if [[ -z "${line//[[:space:]]/}" ]] || [[ "$line" == \#* ]]; then
    skip_count=$(( skip_count + 1 ))
    continue
  fi

  if [[ "$line" != *=* ]]; then
    echo "Skipping invalid line $line_no: $line" >&2
    skip_count=$(( skip_count + 1 ))
    continue
  fi

  key="${line%%=*}"
  value="${line#*=}"

  key="$(trim "$key")"
  value="$(trim "$value")"

  if [[ -z "$key" ]]; then
    echo "Skipping line $line_no with empty key." >&2
    skip_count=$(( skip_count + 1 ))
    continue
  fi

  encoded_key="$(url_encode "$key")"
  payload="$(json_escape "$value")"
  url="${SETTINGS_BASE_URL}/${encoded_key}"

  echo "[$line_no] PUT $key"

  response="$(
    curl --silent --show-error --fail \
      -X PUT "$url" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $token" \
      -d "$payload"
  )"

  success_count=$(( success_count + 1 ))
  printf '%s\n' "$response"
  echo
done < "$SETTINGS_FILE"

echo "Done."
echo "Applied: $success_count"
echo "Skipped: $skip_count"
