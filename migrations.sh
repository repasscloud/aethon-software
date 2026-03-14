#!/usr/bin/bash

dotnet ef migrations add InitialIdentity \
  --project src/Aethon.Data/Aethon.Data.csproj \
  --startup-project src/Aethon.Api/Aethon.Api.csproj \
  --output-dir Migrations

dotnet ef database update \
  --project src/Aethon.Data/Aethon.Data.csproj \
  --startup-project src/Aethon.Api/Aethon.Api.csproj

mkdir -p db/sql

dotnet ef migrations script \
  --idempotent \
  --project src/Aethon.Data/Aethon.Data.csproj \
  --startup-project src/Aethon.Api/Aethon.Api.csproj \
  --output db/sql/001_initial_identity.sql