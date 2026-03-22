#!/usr/bin/env bash
set -euo pipefail

# seed-locations.sh
#
# Logs in, extracts the JWT token from the response JSON,
# then calls the bulk locations endpoint with that token.
#
# Usage:
#   chmod +x seed-locations.sh
#   ./seed-locations.sh
#
# Optional env overrides:
#   AUTH_URL=http://localhost:5201/api/v1/auth/login
#   BULK_URL=http://localhost:5100/api/v1/admin/locations/bulk
#   LOGIN_EMAIL=aethon@localhost.com
#   LOGIN_PASSWORD='Aethon@Admin2026!'

AUTH_URL="${AUTH_URL:-http://localhost:5201/api/v1/auth/login}"
BULK_URL="${BULK_URL:-http://localhost:5201/api/v1/admin/locations/bulk}"
LOGIN_EMAIL="${LOGIN_EMAIL:-aethon@localhost.com}"
LOGIN_PASSWORD="${LOGIN_PASSWORD:-Aethon@Admin2026!}"

if ! command -v curl >/dev/null 2>&1; then
  echo "curl is required but not installed."
  exit 1
fi

login_payload="$(cat <<JSON
{
  "email": "$LOGIN_EMAIL",
  "password": "$LOGIN_PASSWORD"
}
JSON
)"

locations_payload="$(cat <<'JSON'
[
  {"displayName":"Sydney, NSW, Australia","city":"Sydney","state":"New South Wales","country":"Australia","countryCode":"AU","latitude":-33.8688,"longitude":151.2093,"sortOrder":1},
  {"displayName":"Melbourne, VIC, Australia","city":"Melbourne","state":"Victoria","country":"Australia","countryCode":"AU","latitude":-37.8136,"longitude":144.9631,"sortOrder":2},
  {"displayName":"Brisbane, QLD, Australia","city":"Brisbane","state":"Queensland","country":"Australia","countryCode":"AU","latitude":-27.4698,"longitude":153.0251,"sortOrder":3},
  {"displayName":"Perth, WA, Australia","city":"Perth","state":"Western Australia","country":"Australia","countryCode":"AU","latitude":-31.9505,"longitude":115.8605,"sortOrder":4},
  {"displayName":"Adelaide, SA, Australia","city":"Adelaide","state":"South Australia","country":"Australia","countryCode":"AU","latitude":-34.9285,"longitude":138.6007,"sortOrder":5},
  {"displayName":"Gold Coast, QLD, Australia","city":"Gold Coast","state":"Queensland","country":"Australia","countryCode":"AU","latitude":-28.0167,"longitude":153.4000,"sortOrder":6},
  {"displayName":"Canberra, ACT, Australia","city":"Canberra","state":"Australian Capital Territory","country":"Australia","countryCode":"AU","latitude":-35.2809,"longitude":149.1300,"sortOrder":7},
  {"displayName":"Newcastle, NSW, Australia","city":"Newcastle","state":"New South Wales","country":"Australia","countryCode":"AU","latitude":-32.9283,"longitude":151.7817,"sortOrder":8},
  {"displayName":"Wollongong, NSW, Australia","city":"Wollongong","state":"New South Wales","country":"Australia","countryCode":"AU","latitude":-34.4278,"longitude":150.8931,"sortOrder":9},
  {"displayName":"Remote / Work from Home","city":null,"state":null,"country":"Australia","countryCode":"AU","latitude":-25.2744,"longitude":133.7751,"sortOrder":100}
]
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
echo "Posting locations to: $BULK_URL"

bulk_response="$(
  curl --silent --show-error --fail \
    -X POST "$BULK_URL" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $token" \
    -d "$locations_payload"
)"

echo "Bulk seed completed."
echo
printf '%s\n' "$bulk_response"
