#!/usr/bin/env bash
set -euo pipefail

# format-geonames-au.sh
#
# Takes a GeoNames country dump text file like AU.txt and writes:
# 1. a tab-delimited file with headers
# 2. an optional CSV version with quoted fields
#
# Usage:
#   chmod +x format-geonames-au.sh
#   ./format-geonames-au.sh AU.txt
#
# Optional:
#   ./format-geonames-au.sh AU.txt output-dir

INPUT_FILE="${1:-}"
OUTPUT_DIR="${2:-.}"

if [[ -z "$INPUT_FILE" ]]; then
  echo "Usage: $0 <AU.txt> [output-dir]"
  exit 1
fi

if [[ ! -f "$INPUT_FILE" ]]; then
  echo "Input file not found: $INPUT_FILE"
  exit 1
fi

mkdir -p "$OUTPUT_DIR"

INPUT_BASENAME="$(basename "$INPUT_FILE")"
INPUT_NAME="${INPUT_BASENAME%.*}"

TSV_OUTPUT="$OUTPUT_DIR/${INPUT_NAME}.with-headers.tsv"
CSV_OUTPUT="$OUTPUT_DIR/${INPUT_NAME}.with-headers.csv"

# GeoNames standard geoname dump columns
HEADER_TSV=$'geonameid\tname\tasciiname\talternatenames\tlatitude\tlongitude\tfeature_class\tfeature_code\tcountry_code\tcc2\tadmin1_code\tadmin2_code\tadmin3_code\tadmin4_code\tpopulation\televation\tdem\ttimezone\tmodification_date'

echo "Writing TSV with headers: $TSV_OUTPUT"
{
  printf '%s\n' "$HEADER_TSV"
  cat "$INPUT_FILE"
} > "$TSV_OUTPUT"

echo "Writing CSV with headers: $CSV_OUTPUT"
awk -F'\t' '
BEGIN {
  OFS=","
  print \
    "\"geonameid\"," \
    "\"name\"," \
    "\"asciiname\"," \
    "\"alternatenames\"," \
    "\"latitude\"," \
    "\"longitude\"," \
    "\"feature_class\"," \
    "\"feature_code\"," \
    "\"country_code\"," \
    "\"cc2\"," \
    "\"admin1_code\"," \
    "\"admin2_code\"," \
    "\"admin3_code\"," \
    "\"admin4_code\"," \
    "\"population\"," \
    "\"elevation\"," \
    "\"dem\"," \
    "\"timezone\"," \
    "\"modification_date\""
}
{
  for (i = 1; i <= NF; i++) {
    gsub(/"/, "\"\"", $i)
    $i = "\"" $i "\""
  }
  print $1,$2,$3,$4,$5,$6,$7,$8,$9,$10,$11,$12,$13,$14,$15,$16,$17,$18,$19
}
' "$INPUT_FILE" > "$CSV_OUTPUT"

echo
echo "Done."
echo "TSV: $TSV_OUTPUT"
echo "CSV: $CSV_OUTPUT"
