---
title: Public Endpoints
description: Unauthenticated endpoints for job search, organisation profiles, and job applications
weight: 20
---

# Public Endpoints

All endpoints under `/api/v1/public` are anonymous — no authentication token is required.

Some endpoints behave differently when called with a valid JWT: school-leaver-targeted jobs are only visible to authenticated job seekers with `AgeGroup = SchoolLeaver`. The token is optional; unauthenticated callers simply see a filtered result set.

---

## Feature flags

**`GET`** `/api/v1/public/features`

> 🔓 **Public**

Returns publicly safe feature flag states. Use this to determine whether launch promotions or gated features are active.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `launchPromotionEnabled` | `boolean` | Whether the launch promotion (free job credits) is currently active. |

---

## Location search

**`GET`** `/api/v1/public/locations`

> 🔓 **Public**

Searches the curated locations table. Returns up to 10 results. Used for location typeahead inputs on forms.

#### Query parameters

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `q` | `string` | No | Search term matched against display name, city, state, and country. |

#### Response `200 OK`

Array of location objects:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Location ID. |
| `displayName` | `string` | No | Human-readable label (e.g. "Sydney, NSW, Australia"). |
| `city` | `string` | Yes | City component. |
| `state` | `string` | Yes | State/region component. |
| `country` | `string` | Yes | Country component. |
| `countryCode` | `string` | Yes | ISO 3166-1 alpha-2 country code. |
| `latitude` | `number` | Yes | GPS latitude. |
| `longitude` | `number` | Yes | GPS longitude. |

---

## Job location suggestions

**`GET`** `/api/v1/public/jobs/locations`

> 🔓 **Public**

Returns location typeahead suggestions derived from currently active (published, non-expired) job postings. Useful for filtering the job board by actual job locations.

#### Query parameters

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `q` | `string` | No | Search term. |

#### Response `200 OK`

Array of location suggestion strings.

---

## Search jobs

**`GET`** `/api/v1/public/jobs`

> 🔓 **Public** *(optional JWT for school-leaver visibility)*

Returns published, publicly visible jobs. Results are capped at 200 per request. School-leaver-targeted jobs are hidden from unauthenticated callers and from callers whose job seeker profile has `AgeGroup ≠ SchoolLeaver`.

#### Query parameters

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `city` | `string` | No | Filter by city name (partial match). |
| `dateRange` | `DateRangeFilter` | No | Filter by publish date. See [DateRangeFilter](../models/enums.md#daterangefilter). |
| `category` | `JobCategory` | No | Filter by industry/role category. See [JobCategory](../models/enums.md#jobcategory). |
| `region` | `JobRegion` | No | Filter by geographic region. See [JobRegion](../models/enums.md#jobregion). |
| `country` | `string` | No | Filter by country name (partial match). |
| `keywords` | `string` | No | Full-text match against title, description, and keyword tags. |
| `organisationSlug` | `string` | No | Filter to a specific organisation by slug. |
| `salaryMin` | `number` | No | Minimum salary filter (matches against `salaryTo` or `oteTo`). |
| `salaryMax` | `number` | No | Maximum salary filter (matches against `salaryFrom`). |
| `verifiedOnly` | `boolean` | No | `true` to show only verified employer jobs. Default `false`. |
| `workplaceType` | `WorkplaceType` | No | Filter by workplace arrangement. See [WorkplaceType](../models/enums.md#workplacetype). |
| `immediateStart` | `boolean` | No | `true` to show only immediate start roles. Default `false`. |
| `latitude` | `number` | No | Radius search centre latitude. Requires `longitude`. |
| `longitude` | `number` | No | Radius search centre longitude. Requires `latitude`. |
| `radiusKm` | `number` | No | Radius in kilometres. Default `25`. |

#### Response `200 OK`

Array of `PublicJobListItemDto`:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Job ID. |
| `title` | `string` | No | Job title. |
| `summary` | `string` | Yes | Short summary for listing cards. |
| `organisationName` | `string` | No | Employer or agency name. |
| `organisationSlug` | `string` | Yes | Organisation's public URL slug. |
| `organisationLogoUrl` | `string` | Yes | Logo URL. Only populated when `includeCompanyLogo` is true. |
| `organisationIsVerified` | `boolean` | No | Whether the posting organisation is verified. |
| `department` | `string` | Yes | Department or business unit. |
| `locationText` | `string` | Yes | Free-text location label. |
| `locationLatitude` | `number` | Yes | GPS latitude for map display. |
| `locationLongitude` | `number` | Yes | GPS longitude. |
| `distanceKm` | `number` | Yes | Distance from search centre. Populated only on radius searches. |
| `workplaceType` | `WorkplaceType` | No | `OnSite`, `Hybrid`, or `Remote`. |
| `employmentType` | `EmploymentType` | No | `FullTime`, `PartTime`, `Contract`, etc. |
| `salaryFrom` | `number` | Yes | Salary range lower bound. |
| `salaryTo` | `number` | Yes | Salary range upper bound. |
| `salaryCurrency` | `CurrencyCode` | Yes | Salary currency. |
| `hasCommission` | `boolean` | No | Whether commission applies. |
| `oteFrom` | `number` | Yes | On-target earnings lower bound. |
| `oteTo` | `number` | Yes | On-target earnings upper bound. |
| `publishedUtc` | `string (ISO 8601)` | Yes | When the job was published. |
| `category` | `JobCategory` | Yes | Industry/role category. |
| `regions` | `JobRegion[]` | No | List of associated regions. |
| `countries` | `string[]` | No | List of associated countries. |
| `benefitsTags` | `string[]` | No | Benefit labels. |
| `isHighlighted` | `boolean` | No | Whether this listing has a visual highlight. |
| `isImmediateStart` | `boolean` | No | Whether the role has an immediate start. |
| `includeCompanyLogo` | `boolean` | No | Whether the logo is displayed. |
| `isSuitableForSchoolLeavers` | `boolean` | No | Job accepts school leavers (16–18) alongside adults. |
| `isSchoolLeaverTargeted` | `boolean` | No | Job specifically targets school leavers — only visible to school leavers. |

---

## Get job details

**`GET`** `/api/v1/public/jobs/{jobId}`

> 🔓 **Public** *(optional JWT for school-leaver visibility)*

Returns full details of a single published job. Returns `404` for school-leaver-targeted jobs when the caller is not an authenticated school leaver.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job identifier. |

#### Response `200 OK`

`PublicJobDetailDto` — see [Data Schemas](../models/schemas.md#publicjobdetaildto).

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Job found and caller is eligible to view it. |
| `404 Not Found` | Job not found, not published, expired, or caller is not eligible. |

---

## Apply to a job (in-platform)

**`POST`** `/api/v1/public/jobs/{jobId}/apply`

> 🔒 **Authenticated** — job seeker only

Submits a job application via the platform. The applicant must have:

- A complete job seeker profile
- `AgeGroup` confirmed (not `NotSpecified`)
- Age group eligibility for the specific job (see [Age Policy](../README.md#age-policy))

The system checks for duplicate applications and validates screening answer eligibility automatically.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job to apply for. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `coverLetter` | `string` | No | Cover letter text. |
| `source` | `string` | No | Application source label. Defaults to `"AethonPublicBoard"`. |
| `screeningAnswersJson` | `string` | No | JSON-serialised answers to the job's screening questions. |

#### Response `200 OK`

Returns the created application. See [Application object](../models/schemas.md#jobapplication).

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Application submitted. |
| `400 Bad Request` | Age group not confirmed, age group ineligible, applications closed, or duplicate. |
| `401 Unauthorized` | Not authenticated. |
| `404 Not Found` | Job not found or not published. |

---

## Apply to a job (email)

**`POST`** `/api/v1/public/jobs/{jobId}/apply-email`

> 🔓 **Public**

Sends the applicant's CV directly to the employer by email. No account required. Used for jobs where the employer has enabled email-based applications.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job to apply for. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | `string` | Yes | Applicant's full name. |
| `email` | `string` | Yes | Applicant's email address. |
| `phone` | `string` | No | Applicant's phone number. |
| `coverLetter` | `string` | No | Cover letter text. |
| `resumeFileName` | `string` | No | File name of attached resume. |
| `resumeBase64` | `string` | No | Base64-encoded resume file content. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `message` | `string` | Confirmation that the application email was sent. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Email sent. |
| `404 Not Found` | Job not found or not published. |
| `503 Service Unavailable` | Email service unavailable. |

---

## Get public job seeker profile

**`GET`** `/api/v1/public/job-seekers/{identifier}`

> 🔓 **Public** *(access rules apply)*

Returns a job seeker's public profile. Access depends on `ProfileVisibility`:

| Visibility | Who can access |
| --- | --- |
| `Public` | Anyone — `identifier` can be the slug |
| `Unlisted` | Authenticated employers, recruiters, and admins — `identifier` must be the user UUID |
| `Private` | Admin and support staff only |

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `identifier` | `string` | Profile slug (public) or user UUID (unlisted/private). |

#### Response `200 OK`

See [PublicJobSeekerProfileDto](../models/schemas.md#publicjobseekerprofildto).

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Profile returned. |
| `404 Not Found` | Profile not found or visibility rules block access. |

---

## Get organisation profile

**`GET`** `/api/v1/public/organisations/{slug}`

> 🔓 **Public**

Returns a publicly visible organisation profile (employers and agencies with `IsPublicProfileEnabled = true`).

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `slug` | `string` | Organisation URL slug. |

#### Response `200 OK`

See [PublicOrganisationProfileDto](../models/schemas.md#publicorganisationprofiledto).

---

## Get organisation team

**`GET`** `/api/v1/public/organisations/{slug}/team`

> 🔓 **Public**

Returns team members whose profiles are publicly visible within the organisation.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `slug` | `string` | Organisation URL slug. |

#### Response `200 OK`

Array of team member summaries.

---

## Get team member profile

**`GET`** `/api/v1/public/organisations/{slug}/team/{memberSlug}`

> 🔓 **Public**

Returns a single team member's public profile within an organisation.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `slug` | `string` | Organisation URL slug. |
| `memberSlug` | `string` | Team member's profile slug. |

#### Response `200 OK`

Team member detail object.
