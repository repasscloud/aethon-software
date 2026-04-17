#!/usr/bin/env bash
set -euo pipefail
docker build -f .docker/Dockerfile.builder -t repasscloud/aethon-web-builder:mcr-dotnet-sdk-10.0 . && \
docker push repasscloud/aethon-web-builder:mcr-dotnet-sdk-10.0