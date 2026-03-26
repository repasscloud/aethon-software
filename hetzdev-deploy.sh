#!/usr/bin/env bash
set -e

docker compose down > /dev/null 2>&1 || true
docker volume rm aethon-software_postgres_data > /dev/null 2>&1 || true
docker compose up --build -d
