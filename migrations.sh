#!/bin/bash

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