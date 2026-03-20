#!/bin/bash

set -e

CERT_FILE="dp-cert.pfx"

openssl req -x509 \
  -newkey rsa:2048 \
  -keyout /tmp/dp-key.pem \
  -out /tmp/dp-cert.pem \
  -days 3650 \
  -nodes \
  -subj "/CN=Aethon-DataProtection" \
  2>/dev/null

openssl pkcs12 -export \
  -out "$CERT_FILE" \
  -inkey /tmp/dp-key.pem \
  -in /tmp/dp-cert.pem \
  -passout pass: \
  2>/dev/null

rm -f /tmp/dp-key.pem /tmp/dp-cert.pem

CERT_BASE64=$(base64 < "$CERT_FILE")
rm -f "$CERT_FILE"

echo ""
echo "Add the following line to your .env file:"
echo ""
echo "DataProtection__CertBase64=${CERT_BASE64}"
echo ""
