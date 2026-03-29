#!/usr/bin/env bash
set -euo pipefail

# seed-locations.sh
#
# Logs in, extracts the JWT token from the response JSON,
# then calls the bulk locations endpoint with the JSON payload
# loaded from a file passed as the first argument.
#
# Usage:
#   chmod +x seed-locations.sh
#   ./seed-locations.sh ./AU.geoname.json
#
# Optional env overrides:
#   AUTH_URL=http://localhost:5201/api/v1/auth/login
#   BULK_URL=http://localhost:5201/api/v1/admin/locations/bulk
#   LOGIN_EMAIL=secret-user@example.com
#   LOGIN_PASSWORD='Super-Secret-P4ssw0rd!'

AUTH_URL="${AUTH_URL:-https://dev-api.app.aethonsoftware.com/api/v1/auth/login}"
BULK_URL="${BULK_URL:-https://dev-api.app.aethonsoftware.com/api/v1/admin/locations/bulk}"
LOGIN_EMAIL="${LOGIN_EMAIL:-secret-user@example.com}"
LOGIN_PASSWORD="${LOGIN_PASSWORD:-Super-Secret-P4ssw0rd!}"

if ! command -v curl >/dev/null 2>&1; then
  echo "curl is required but not installed."
  exit 1
fi

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <payload.json>"
  exit 1
fi

JSON_FILE="$1"

if [[ ! -f "$JSON_FILE" ]]; then
  echo "JSON file does not exist: $JSON_FILE"
  exit 1
fi

if [[ ! -r "$JSON_FILE" ]]; then
  echo "JSON file is not readable: $JSON_FILE"
  exit 1
fi

login_payload="$(cat <<JSON
{
  "email": "$LOGIN_EMAIL",
  "password": "$LOGIN_PASSWORD"
}
JSON
)"

echo "Logging in at: $AUTH_URL"

login_response="$(
  curl --silent --show-error --fail \
    -X POST "$AUTH_URL" \
    -H "Content-Type: application/json" \
    -d "$login_payload"
)"

echo "Login response received."

token=""
if command -v jq >/dev/null 2>&1; then
  token="$(printf '%s' "$login_response" | jq -r '.token // empty')"
else
  token="$(printf '%s' "$login_response" | sed -n 's/.*"token":"\([^"]*\)".*/\1/p')"
fi

if [[ -z "$token" ]]; then
  echo "Failed to extract token from login response."
  echo "Raw response:"
  printf '%s\n' "$login_response"
  exit 1
fi

echo "Token extracted."
echo "Posting locations from file: $JSON_FILE"
echo "Posting locations to: $BULK_URL"

bulk_response="$(
  curl --silent --show-error --fail \
    -X POST "$BULK_URL" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $token" \
    --data-binary "@$JSON_FILE"
)"

echo "Bulk seed completed."
echo
printf '%s\n' "$bulk_response"
