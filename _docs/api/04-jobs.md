---
title: Jobs
description: Create, update, publish, and manage job postings (employer and recruiter)
weight: 40
---

# Jobs

All endpoints under `/api/v1/jobs` require authentication. Access is restricted to the user's organisation — you cannot view or modify another organisation's jobs via these endpoints.

See [Recruiter Jobs](10-recruiter-jobs.md) for the recruiter-specific draft-and-submit workflow.

---

## Create job

**`POST`** `/api/v1/jobs`

> 🔒 **Authenticated** — company or recruiter member

Creates a new job in `Draft` status. The job is not publicly visible until published.

The owning organisation is automatically derived from the caller's active organisation membership — you do not supply it.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `title` | `string` | Yes | Job title. Max 250 chars. |
| `summary` | `string` | Yes | Short listing summary. Min 50, max 300 chars. |
| `department` | `string` | No | Department or business unit. |
| `locationText` | `string` | Yes | Free-text location shown to candidates. Max 250 chars. |
| `locationCity` | `string` | No | Structured city (from Google Places). |
| `locationState` | `string` | No | Structured state/region. |
| `locationCountry` | `string` | No | Structured country name. |
| `locationCountryCode` | `string` | No | ISO 3166-1 alpha-2 country code. |
| `locationLatitude` | `number` | No | GPS latitude for radius search. |
| `locationLongitude` | `number` | No | GPS longitude. |
| `locationPlaceId` | `string` | No | Google Places place_id. |
| `workplaceType` | `WorkplaceType` | Yes | `OnSite`, `Hybrid`, or `Remote`. |
| `employmentType` | `EmploymentType` | Yes | `FullTime`, `PartTime`, `Contract`, `Temporary`, `Casual`, or `Internship`. |
| `description` | `string` | Yes | Full job description. Max 20,000 chars. |
| `requirements` | `string` | No | Candidate requirements. |
| `benefits` | `string` | No | Benefits and perks. |
| `salaryFrom` | `number` | No | Salary range lower bound. |
| `salaryTo` | `number` | No | Salary range upper bound. |
| `salaryCurrency` | `CurrencyCode` | Cond. | Required when salary bounds are set. |
| `hasCommission` | `boolean` | No | Whether commission applies. |
| `oteFrom` | `number` | No | On-target earnings lower bound. |
| `oteTo` | `number` | No | On-target earnings upper bound. |
| `applyByUtc` | `string (ISO 8601)` | No | Application deadline. |
| `postingExpiresUtc` | `string (ISO 8601)` | No | When the listing auto-expires. |
| `category` | `JobCategory` | Yes | Industry/role category. |
| `regions` | `JobRegion[]` | No | Geographic regions. |
| `countries` | `string[]` | No | Countries for the role. |
| `keywords` | `string` | No | ATS matching keywords. |
| `isImmediateStart` | `boolean` | No | Immediate start available. |
| `externalApplicationUrl` | `string` | No | External apply URL (bypasses in-platform application). |
| `applicationEmail` | `string` | No | Email address for email-based applications. |
| `applicationSpecialRequirements` | `string` | No | Special requirements shown to applicants. |
| `screeningQuestionsJson` | `string` | No | JSON-serialised screening questions config. |
| `isSuitableForSchoolLeavers` | `boolean` | No | Job accepts school leavers (16–18) alongside adults. Default `false`. |
| `isSchoolLeaverTargeted` | `boolean` | No | Job specifically targets school leavers. Only visible to school leavers. Default `false`. |
| `videoYouTubeId` | `string` | No | YouTube video ID to embed on the listing. |
| `videoVimeoId` | `string` | No | Vimeo video ID (mutually exclusive with YouTube). |
| `postingTier` | `JobPostingTier` | No | `Standard` (default) or `Premium`. |

#### Response `200 OK`

`JobDetailDto` — see [Data Schemas](../models/schemas.md#jobdetaildto).

---

## Get job

**`GET`** `/api/v1/jobs/{jobId}`

> 🔒 **Authenticated**

Returns full details of a job owned by the caller's organisation.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job identifier. |

#### Response `200 OK`

`JobDetailDto`.

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Job found. |
| `404 Not Found` | Not found or not owned by caller's organisation. |

---

## List organisation's jobs

**`GET`** `/api/v1/jobs/my-org`

> 🔒 **Authenticated**

Returns all jobs for the caller's active organisation, ordered by most recently updated. Includes all statuses (Draft, Published, Closed, etc.).

#### Response `200 OK`

Array of `JobListItemDto` — see [Data Schemas](../models/schemas.md#joblistitemdto).

---

## Update job

**`PUT`** `/api/v1/jobs/{jobId}`

> 🔒 **Authenticated**

Updates a job. All supplied fields are replaced. Omitted fields retain their current values. Only jobs in `Draft` or `OnHold` status can typically be freely updated — published jobs follow a stricter workflow.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job identifier. |

#### Request body

Same fields as [Create job](#create-job). All optional.

#### Response `200 OK`

Updated `JobDetailDto`.

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Updated. |
| `402 Payment Required` | Premium feature attempted without sufficient credits. |
| `404 Not Found` | Not found or access denied. |

---

## Publish job

**`POST`** `/api/v1/jobs/{jobId}/publish`

> 🔒 **Authenticated**

Publishes a job from `Draft`, `Approved`, or `OnHold` status. Attempts to consume a job posting credit from the organisation's balance. Returns `402` if no credits are available — redirect the user to the [billing checkout](07-billing.md).

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job identifier. |

#### Response `200 OK`

Published `JobDetailDto`.

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Job published. |
| `402 Payment Required` | Insufficient job posting credits. |
| `404 Not Found` | Not found or access denied. |
| `422 Unprocessable Entity` | Status transition not permitted. |

---

## Apply add-ons

**`POST`** `/api/v1/jobs/{jobId}/addons`

> 🔒 **Authenticated**

Applies paid promotional add-ons to an existing published job. Each add-on consumes billing credits. Returns `402` on insufficient balance.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job identifier. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `addHighlight` | `boolean` | No | Enable coloured highlight on listing card. |
| `highlightColour` | `string` | Cond. | Hex colour (e.g. `#FFD700`). Required when `addHighlight = true`. |
| `addAiMatching` | `boolean` | No | Enable AI candidate matching. |
| `videoYouTubeId` | `string` | No | Embed a YouTube video. |
| `videoVimeoId` | `string` | No | Embed a Vimeo video. |
| `stickyDuration` | `integer` | No | Sticky-to-top duration in days. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `isHighlighted` | `boolean` | Current highlight status. |
| `highlightColour` | `string` | Active highlight colour. |
| `hasAiCandidateMatching` | `boolean` | AI matching status. |
| `stickyUntilUtc` | `string (ISO 8601)` | Sticky expiry. |
| `videoYouTubeId` | `string` | Active YouTube ID. |
| `videoVimeoId` | `string` | Active Vimeo ID. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Add-ons applied. |
| `402 Payment Required` | Insufficient credits for the requested add-ons. |

---

## Close job

**`POST`** `/api/v1/jobs/{jobId}/close`

> 🔒 **Authenticated**

Closes a published job. No more applications will be accepted. The job listing is removed from the public board.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job identifier. |

#### Response `200 OK`

Updated `JobDetailDto` with `status = Closed`.

---

## Return to draft

**`POST`** `/api/v1/jobs/{jobId}/return-to-draft`

> 🔒 **Authenticated**

Returns a published or closed job to `Draft` status for editing. The job is removed from the public board immediately.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job identifier. |

#### Response `200 OK`

Updated `JobDetailDto` with `status = Draft`.

---

## Put on hold

**`POST`** `/api/v1/jobs/{jobId}/put-on-hold`

> 🔒 **Authenticated**

Temporarily hides a published job from the public board without closing it. The job can be re-published later.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job identifier. |

#### Response `200 OK`

Updated `JobDetailDto` with `status = OnHold`.

---

## Get applications for a job

**`GET`** `/api/v1/jobs/{jobId}/applications`

> 🔒 **Authenticated**

Returns a paginated list of applications received for a specific job. Accessible to organisation members with sufficient permissions.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job identifier. |

#### Query parameters

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `status` | `ApplicationStatus` | No | — | Filter by application status. |
| `page` | `integer` | No | `1` | Page number. |
| `pageSize` | `integer` | No | `20` | Items per page. |

#### Response `200 OK`

Paginated list of `EmployerJobApplicationListItemDto` — see [Data Schemas](../models/schemas.md#employerjobapplicationlistitemdto).
