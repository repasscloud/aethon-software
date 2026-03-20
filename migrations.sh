#!/bin/bash

set -e

if [ -d "src/Aethon.Data/Migrations" ]; then
  echo "Removing src/Aethon.Data/Migrations..."
  rm -rf src/Aethon.Data/Migrations
fi

if [ -d "db" ]; then
  echo "Removing db/..."
  rm -rf db
fi

dotnet ef migrations add InitDb \
  --project src/Aethon.Data/Aethon.Data.csproj \
  --startup-project src/Aethon.Api/Aethon.Api.csproj \
  --output-dir Migrations

mkdir -p db/sql

dotnet ef migrations script \
  --idempotent \
  --project src/Aethon.Data/Aethon.Data.csproj \
  --startup-project src/Aethon.Api/Aethon.Api.csproj \
  --output db/sql/001_init_db.sql

echo "Done."
