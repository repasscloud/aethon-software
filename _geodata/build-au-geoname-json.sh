#!/usr/bin/env bash
set -euo pipefail

input_file="${1:-AU.with-headers.csv}"
output_file="AU.geoname.json"

python3 - "$input_file" "$output_file" <<'PY'
import csv
import json
import sys
from decimal import Decimal

input_file = sys.argv[1]
output_file = sys.argv[2]

allowed_feature_codes = {
    "PPL", "PPLA", "PPLA2", "PPLA3", "PPLA4", "PPLA5", "PPLC", "PPLL", "PPLX"
}

timezone_to_state = {
    "Australia/Adelaide": "South Australia",
    "Australia/Brisbane": "Queensland",
    "Australia/Broken_Hill": "New South Wales",
    "Australia/Darwin": "Northern Territory",
    "Australia/Eucla": "Western Australia",
    "Australia/Hobart": "Tasmania",
    "Australia/Lindeman": "Queensland",
    "Australia/Lord_Howe": "New South Wales",
    "Australia/Melbourne": "Victoria",
    "Australia/Perth": "Western Australia",
    "Australia/Sydney": "New South Wales",
}

results = []

with open(input_file, "r", encoding="utf-8-sig", newline="") as f:
    reader = csv.DictReader(f)

    for row in reader:
        country_code = (row.get("country_code") or "").strip()
        feature_class = (row.get("feature_class") or "").strip()
        feature_code = (row.get("feature_code") or "").strip()
        timezone = (row.get("timezone") or "").strip()
        city = (row.get("name") or "").strip()

        if country_code != "AU":
            continue

        if feature_class != "P":
            continue

        if feature_code not in allowed_feature_codes:
            continue

        state = timezone_to_state.get(timezone)
        if not state:
            # Skip rows with blank/unknown timezone since state cannot be derived
            continue

        latitude_raw = (row.get("latitude") or "").strip()
        longitude_raw = (row.get("longitude") or "").strip()

        if not latitude_raw or not longitude_raw:
            continue

        latitude = float(Decimal(latitude_raw))
        longitude = float(Decimal(longitude_raw))

        item = {
            "displayName": f"{city}, {state}, Australia",
            "city": city,
            "state": state,
            "country": "Australia",
            "countryCode": "AU",
            "latitude": latitude,
            "longitude": longitude,
            "sortOrder": 0
        }

        results.append(item)

with open(output_file, "w", encoding="utf-8") as f:
    json.dump(results, f, ensure_ascii=False, indent=2)

print(f"Wrote {len(results)} records to {output_file}")
PY
