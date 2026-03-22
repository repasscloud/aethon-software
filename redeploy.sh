#!/bin/bash

set -e

BUILDER_IMAGE="aethon-build:mcr-dotnet-sdk-10.0"
BUILDER_DOCKERFILE="./.docker/Dockerfile.builder"

echo "==> Verifying build..."
dotnet build --no-restore -p:TreatWarningsAsErrors=false
echo "==> Build OK"

echo "==> Tearing down existing containers, images, and volumes..."
docker compose down --rmi local --remove-orphans --volumes

echo "==> Checking builder image: ${BUILDER_IMAGE}"
if [ -n "$(docker image ls "${BUILDER_IMAGE}" -q)" ]; then
  echo "==> Builder image exists"
else
  echo "==> Builder image missing, building it now..."
  docker build -f "${BUILDER_DOCKERFILE}" -t "${BUILDER_IMAGE}" .
  echo "==> Builder image built"
fi

echo "==> Building and starting stack..."
docker compose up --build -d

echo "==> Done. Stack is up."
echo ""
echo "  Web: http://localhost:5200"
echo "  API: http://localhost:5201"