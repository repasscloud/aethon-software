---
title: Import Feed
description: External job import ingestion endpoints — ingest single or bulk jobs from third-party feeds via API key auth
weight: 160
---

# Import Feed

Endpoints under `/api/v1/import` allow an external cron job or feed ingester to push job postings into the platform without a user account.

> 🔑 **API key auth only** — no JWT, no login. Pass the key in the `X-Import-Api-Key` header on every request.

The key is resolved in this order:

1. **DB** — `Import.ApiKey` system setting (set/rotated via [`POST /api/v1/admin/settings/import-api-key/rotate`](15-admin.md#rotate-import-api-key))
2. **Fallback** — `IMPORT_API_KEY` environment variable

---

## Ingest single job

**`POST`** `/api/v1/import/jobs`

Creates one job from an external feed. The job is immediately published and publicly visible.

#### Request headers

| Header | Required | Description |
| --- | --- | --- |
| `X-Import-Api-Key` | yes | Import API key |
| `Content-Type` | yes | `application/json` |

#### Request body

```json
{
  "sourceSite": "remoteok.com",
  "externalId": "12345",
  "companyName": "Microsoft",
  "companyLogoUrl": "https://cdn.remoteok.com/logos/microsoft.png",

  "title": "Senior Software Engineer",
  "description": "<p>Full HTML description here...</p>",
  "workplaceType": "Remote",
  "employmentType": "FullTime",

  "externalApplicationUrl": "https://remoteok.com/apply/12345",

  "category": "ITSoftware",
  "keywords": "c# dotnet azure",
  "regions": ["Oceania", "NorthAmerica"],
  "countries": ["Australia", "United States"],

  "summary": "Optional short blurb. Auto-generated from description if omitted.",
  "requirements": "<p>Optional HTML requirements...</p>",
  "benefits": "<p>Optional HTML benefits...</p>",
  "department": "Engineering",

  "salaryFrom": 120000,
  "salaryTo": 160000,
  "salaryCurrency": "AUD",

  "publishedUtc": "2026-03-28T10:00:00Z",
  "postingExpiresUtc": "2026-06-28T00:00:00Z",

  "locationText": "Sydney NSW, Australia",
  "locationCity": "Sydney",
  "locationState": "New South Wales",
  "locationCountry": "Australia",
  "locationCountryCode": "AU",
  "locationLatitude": -33.8688,
  "locationLongitude": 151.2093,

  "slug": "senior-software-engineer-microsoft"
}
```

#### Field reference

| Field | Required | Notes |
| --- | --- | --- |
| `sourceSite` | **yes** | Short identifier for the source feed (e.g. `"remoteok.com"`). Used to namespace org names and the deduplication key. |
| `externalId` | **yes** | The ID this job has in the source system. |
| `companyName` | **yes** | Used to name/find the import organisation. |
| `title` | **yes** | |
| `description` | **yes** | HTML accepted — rendered as-is on the job detail page. |
| `workplaceType` | **yes** | `OnSite`, `Hybrid`, `Remote` |
| `employmentType` | **yes** | `FullTime`, `PartTime`, `Contract`, `Temporary`, `Casual`, `Internship` |
| `externalApplicationUrl` | **yes** | Where the Apply button sends the candidate. All imported jobs use an external URL — email applications are not supported. |
| `companyLogoUrl` | no | URL to the company logo image. Stored on the import org and shown on the job listing. |
| `category` | no | See [JobCategory enum](../models/enums.md#jobcategory). |
| `keywords` | no | Space or comma-separated keywords for search matching. |
| `regions` | no | JSON array of `JobRegion` values: `Africa`, `Asia`, `Europe`, `LatinAmerica`, `MiddleEast`, `NorthAmerica`, `Oceania`, `Worldwide`. |
| `countries` | no | JSON array of country name strings. |
| `summary` | no | Short plain-text blurb for job cards. Auto-generated from the first 200 characters of `description` (HTML stripped) if omitted. |
| `requirements` | no | HTML accepted. |
| `benefits` | no | HTML accepted. |
| `department` | no | |
| `salaryFrom` | no | Lower bound of salary range. Provide all three salary fields together or none. |
| `salaryTo` | no | Upper bound of salary range. |
| `salaryCurrency` | no | `AUD`, `USD`, `EUR`, `GBP`, `NZD` |
| `publishedUtc` | no | When the job was originally published on the source platform. Defaults to the time of ingestion if omitted. |
| `postingExpiresUtc` | no | When the listing should expire and be hidden. |
| `locationText` | no | Free-text location shown on the listing. |
| `locationCity` | no | |
| `locationState` | no | |
| `locationCountry` | no | |
| `locationCountryCode` | no | ISO 3166-1 alpha-2 (e.g. `"AU"`). |
| `locationLatitude` | no | GPS latitude for radius-based search. |
| `locationLongitude` | no | GPS longitude for radius-based search. |
| `locationPlaceId` | no | Google Places `place_id`. |
| `slug` | no | Slug from the source platform. Stored as `ShortUrlCode`. |

#### Response `200 OK`

```json
{
  "jobId": "a1b2c3d4-0000-0000-0000-000000000000",
  "externalReference": "remoteok.com_12345",
  "wasDuplicate": false
}
```

| Field | Type | Description |
| --- | --- | --- |
| `jobId` | `string (GUID)` | The platform job ID. |
| `externalReference` | `string` | The combined deduplication key stored on the job. |
| `wasDuplicate` | `boolean` | `true` when the job already existed and was skipped. The existing `jobId` is returned. |

#### Error responses

| Status | Code | Description |
| --- | --- | --- |
| `401` | `import.unauthorized` | Missing, empty, or incorrect API key. |
| `400` | `import.source_site_required` | `sourceSite` was blank. |
| `400` | `import.external_id_required` | `externalId` was blank. |
| `400` | `import.company_name_required` | `companyName` was blank. |
| `400` | `import.title_required` | `title` was blank. |
| `400` | `import.description_required` | `description` was blank. |
| `400` | `import.application_url_required` | `externalApplicationUrl` was blank. |

---

## Ingest bulk jobs

**`POST`** `/api/v1/import/jobs/bulk`

Creates up to 500 jobs in a single request. The request body is a JSON array of job objects using the same shape as the single import.

Per-job failures (validation errors) are absorbed — the remaining jobs in the batch continue to be processed. Only auth failures abort the entire request.

#### Request headers

| Header | Required | Description |
| --- | --- | --- |
| `X-Import-Api-Key` | yes | Import API key |
| `Content-Type` | yes | `application/json` |

#### Request body

```json
[
  {
    "sourceSite": "remoteok.com",
    "externalId": "12345",
    "companyName": "Microsoft",
    "title": "Senior Software Engineer",
    "description": "<p>...</p>",
    "workplaceType": "Remote",
    "employmentType": "FullTime",
    "externalApplicationUrl": "https://remoteok.com/apply/12345"
  },
  {
    "sourceSite": "remoteok.com",
    "externalId": "12346",
    "companyName": "Atlassian",
    "title": "Product Designer",
    "description": "<p>...</p>",
    "workplaceType": "Hybrid",
    "employmentType": "FullTime",
    "externalApplicationUrl": "https://remoteok.com/apply/12346"
  }
]
```

Maximum: **500 jobs per request**.

#### Response `200 OK`

```json
[
  { "jobId": "a1b2c3d4-...", "externalReference": "remoteok.com_12345", "wasDuplicate": false },
  { "jobId": "b2c3d4e5-...", "externalReference": "remoteok.com_12346", "wasDuplicate": true },
  { "jobId": "c3d4e5f6-...", "externalReference": "remoteok.com_12347", "wasDuplicate": false }
]
```

An array entry is only present for jobs that were successfully processed (created or identified as a duplicate). Jobs that fail validation are silently dropped from the response.

#### Error responses

| Status | Code | Description |
| --- | --- | --- |
| `401` | `import.unauthorized` | Missing, empty, or incorrect API key. Aborts the entire batch. |
| `400` | `import.empty` | The request body array was empty. |
| `400` | `import.too_many` | More than 500 jobs were submitted. |

---

## How identifiers work

### Deduplication — `sourceSite` + `externalId`

Before creating a job the handler builds a deduplication key:

```
ExternalReference = "{sourceSite}_{externalId}"
// e.g. "remoteok.com_12345"
```

This value is stored on the `Job.ExternalReference` field. If a job with the same key already exists (and `IsImported = true`), the request is treated as a duplicate — no new record is created and `wasDuplicate: true` is returned.

Re-running the same feed repeatedly is safe.

### Organisation lookup — `sourceSite` + `companyName`

Each unique company per source site maps to one auto-created import organisation:

```
Organisation.Name = "ImportOrg_{sourceSite}_{companyName}"
// e.g. "ImportOrg_remoteok.com_Microsoft"
```

The organisation is looked up by normalised name on every import. If it does not exist it is created with:

- No public profile (`IsPublicProfileEnabled = false`)
- No slug (not discoverable via `/organisations/`)
- `CreatedForUnclaimedCompany = true` on each job
- Logo set from `companyLogoUrl` if provided

If 10 jobs arrive from `remoteok.com` all listing `"Microsoft"` as the company, they all share the same organisation record. The org is created once and reused forever.

### System user

One system user is created per import organisation and linked as owner. This account:

- Has a deterministic internal email (`sys-...@aethon-import.internal`)
- Is never exposed publicly
- Cannot be logged into (random inaccessible password)
- Satisfies the `CreatedByIdentityUserId` foreign key on the job

---

## Behaviour notes

| Concern | Behaviour |
| --- | --- |
| Status | Always `Published` |
| Visibility | Always `Public` |
| Posting tier | Always `Imported` (not selectable in the UI) |
| AI candidate matching | Always `false` |
| Google Jobs structured data | Excluded — imported jobs do not emit JSON-LD |
| Sitemap | Included — imported jobs appear in the public sitemap |
| Apply button | Always routes to `externalApplicationUrl` — shown to all visitors regardless of account type |
| UI badge | **External Listing** badge shown on both the jobs list and the job detail page |
| Salary | Fully optional — the UI salary field is bypassed for imports |
| School leaver flags | Always `false` |
