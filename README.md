# Aethon ATS

Internal applicant tracking system. Blazor Server frontend, .NET 10 minimal API backend, PostgreSQL.

---

## First-time setup

### 1. Generate the data protection certificate

Required once. All instances sharing the `/keys` volume must use the same cert.

```bash
./generate-dp-cert.sh
```

Copy the `DataProtection__CertBase64=...` line it prints — you'll need it in the next step.

### 2. Create your `.env` file

```bash
cp .env.example .env
```

Open `.env` and fill in at minimum:

| Variable | Required | Notes |
|---|---|---|
| `Auth__JwtKey` | Yes | Random string, min 32 chars |
| `DataProtection__CertBase64` | Yes | Output from `generate-dp-cert.sh` |
| `Email__MailerSendApiKey` | No | Leave blank to disable email |
| `Claude__ApiKey` | No | Leave blank to disable resume intelligence |

### 3. Start

```bash
docker compose up --build
```

- **Web UI** → http://localhost:5200
- **API** → http://localhost:5201
- **Swagger** → http://localhost:5201/swagger

Database migrations run automatically on API startup. No manual SQL or migration steps needed.

---

## Subsequent starts

```bash
docker compose up
```

Rebuild images only when code changes:

```bash
docker compose up --build
```

---

## Environment variables reference

### Required

```
Auth__JwtKey                   JWT signing secret — min 32 chars, keep private
DataProtection__CertBase64     PFX cert (base64) for encrypting Data Protection keys
```

### Optional (features disabled if blank)

```
Email__MailerSendApiKey        MailerSend API key — transactional email
Claude__ApiKey                 Anthropic API key — resume intelligence (claude-sonnet-4-6)
```

### Defaults (override in .env if needed)

```
Auth__Issuer                   aethon-api
Auth__Audience                 aethon-web
Email__FromEmail               do-not-reply@repasscloud.com
Email__FromName                Aethon
Email__WebBaseUrl              http://localhost:5200
```

---

## Development (without Docker)

Run the two projects in separate terminals:

```bash
dotnet run --project src/Aethon.Api/Aethon.Api.csproj
dotnet run --project src/Aethon.Web/Aethon.Web.csproj
```

Connection string and JWT config for local dev live in:
`src/Aethon.Api/appsettings.Development.json`

Requires a local PostgreSQL instance (`aethon_dev` database, user `postgres`, password `postgres`).

---

## Adding a new migration

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Aethon.Data \
  --startup-project src/Aethon.Api
```

Migrations are applied automatically on next startup — no manual `database update` needed.

---

## Scripts

| Script | Purpose |
|---|---|
| `generate-dp-cert.sh` | Generate Data Protection certificate (run once) |
| `build.sh` | Clean + restore + build solution |
| `migrations.sh` | **Destructive** — wipe and regenerate all migrations from scratch. Only use this to reset the migration history, not for normal development. |
