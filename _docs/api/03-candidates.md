---
title: Candidates (Job Seekers)
description: Profile management, work history, skills, age group, resume linking, and account deletion for job seekers
weight: 30
---

# Candidates (Job Seekers)

All endpoints under `/api/v1/me` are for authenticated job seekers managing their own data.

> 🔒 **All endpoints require authentication.**

---

## Get my profile

**`GET`** `/api/v1/me/profile`

Returns the complete profile for the authenticated job seeker, including resumes, work history, skills, qualifications, certificates, and languages.

Results are cached for up to 3 minutes. The cache is invalidated when the profile is updated.

#### Response `200 OK`

`CandidateProfileDto` — see [Data Schemas](../models/schemas.md#candidateprofiledto).

Returns an empty profile object (only `userId` populated) if the profile has not yet been created.

---

## Update my profile

**`PUT`** `/api/v1/me/profile`

Creates or updates the candidate profile. Uses upsert semantics — safe to call multiple times.

Age group changes are one-directional: once set to `SchoolLeaver` or `Adult`, the `AgeConfirmedUtc` timestamp is recorded and not overwritten on subsequent updates. Birth date fields (month/year) are only stored for `SchoolLeaver` accounts and are cleared if the age group is later changed to `Adult`.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `firstName` | `string` | No | First name. Max 100 chars. |
| `middleName` | `string` | No | Middle name. Max 100 chars. |
| `lastName` | `string` | No | Last name. Max 100 chars. |
| `ageGroup` | `ApplicantAgeGroup` | No | `NotSpecified`, `SchoolLeaver`, or `Adult`. |
| `birthMonth` | `integer` | Cond. | Required when `ageGroup = SchoolLeaver`. Range 1–12. Do not provide for `Adult`. |
| `birthYear` | `integer` | Cond. | Required when `ageGroup = SchoolLeaver`. 4-digit year. Do not provide for `Adult`. |
| `phoneNumber` | `string` | No | Contact phone. Max 50 chars. |
| `whatsAppNumber` | `string` | No | WhatsApp number. Max 50 chars. |
| `headline` | `string` | No | Profile headline. Max 200 chars. |
| `summary` | `string` | No | Biography. Max 4000 chars. |
| `aboutMe` | `string` | No | Short public intro. Max 1000 chars. |
| `currentLocation` | `string` | No | Current location (free text). Max 250 chars. |
| `preferredLocation` | `string` | No | Preferred work location. Max 250 chars. |
| `availabilityText` | `string` | No | Notice period or availability. Max 250 chars. |
| `linkedInUrl` | `string` | No | LinkedIn profile URL. Max 500 chars. |
| `openToWork` | `boolean` | No | Whether actively seeking work. |
| `desiredSalaryFrom` | `number` | No | Desired salary lower bound. Must be ≥ 0. |
| `desiredSalaryTo` | `number` | No | Desired salary upper bound. Must be ≥ `desiredSalaryFrom`. |
| `desiredSalaryCurrency` | `CurrencyCode` | Cond. | Required when either salary bound is set. |
| `willRelocate` | `boolean` | No | Willing to relocate. |
| `requiresSponsorship` | `boolean` | No | Requires work visa sponsorship. |
| `hasWorkRights` | `boolean` | No | Currently has work rights. |
| `profileVisibility` | `ProfileVisibility` | No | `Private`, `Unlisted`, or `Public`. |
| `isSearchable` | `boolean` | No | Appear in recruiter search results. |
| `slug` | `string` | Cond. | Required when `profileVisibility = Public`. Max 200 chars. Must be unique. |

#### Response `200 OK`

Updated `CandidateProfileDto`.

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Profile saved. |
| `400 Bad Request` | Validation failure (e.g. school leaver missing birth month/year, slug already taken). |

---

## Check slug availability

**`GET`** `/api/v1/me/profile/check-slug`

Checks whether a profile URL slug is available before saving.

#### Query parameters

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `slug` | `string` | Yes | Desired slug to check. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `available` | `boolean` | `true` if the slug is free to use. |
| `slug` | `string` | Normalised (lowercase) version of the input slug. |

---

## Work experience

### List work experiences

**`GET`** `/api/v1/me/profile/work-experiences`

Returns all work experience entries, ordered by sort order then most recent start date.

#### Response `200 OK`

Array of `JobSeekerWorkExperienceDto`:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Entry ID. |
| `jobTitle` | `string` | Yes | Role title. |
| `employerName` | `string` | Yes | Employer name. |
| `startMonth` | `integer` | Yes | Start month (1–12). |
| `startYear` | `integer` | Yes | Start year. |
| `endMonth` | `integer` | Yes | End month (1–12). Null if current. |
| `endYear` | `integer` | Yes | End year. Null if current. |
| `isCurrent` | `boolean` | No | Whether this is the current role. |
| `description` | `string` | Yes | Role description. |
| `sortOrder` | `integer` | No | Display order. |

---

### Add work experience

**`POST`** `/api/v1/me/profile/work-experiences`

#### Request body

Same fields as `JobSeekerWorkExperienceDto` (excluding `id`).

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `id` | `string (UUID)` | ID of the created entry. |

---

### Update work experience

**`PUT`** `/api/v1/me/profile/work-experiences/{id}`

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `id` | `UUID` | Work experience entry ID. |

#### Request body

Same as Add. All fields are replaced.

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Updated. |
| `404 Not Found` | Entry not found or belongs to a different user. |

---

### Delete work experience

**`DELETE`** `/api/v1/me/profile/work-experiences/{id}`

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `id` | `UUID` | Work experience entry ID. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Deleted. |
| `404 Not Found` | Not found. |

---

## Qualifications

### List qualifications

**`GET`** `/api/v1/me/profile/qualifications`

Returns qualifications ordered by sort order.

#### Response `200 OK`

Array of `JobSeekerQualificationDto`:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Entry ID. |
| `title` | `string` | Yes | Qualification title (e.g. "Bachelor of Science"). |
| `institution` | `string` | Yes | Issuing institution. |
| `completedYear` | `integer` | Yes | Year of completion. |
| `description` | `string` | Yes | Additional details. |
| `sortOrder` | `integer` | No | Display order. |

---

### Add qualification

**`POST`** `/api/v1/me/profile/qualifications`

Body: `JobSeekerQualificationDto` (excluding `id`). Response: `{ id: UUID }`.

---

### Update qualification

**`PUT`** `/api/v1/me/profile/qualifications/{id}`

---

### Delete qualification

**`DELETE`** `/api/v1/me/profile/qualifications/{id}`

---

## Certificates

### List certificates

**`GET`** `/api/v1/me/profile/certificates`

#### Response `200 OK`

Array of `JobSeekerCertificateDto`:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Entry ID. |
| `name` | `string` | Yes | Certificate name. |
| `issuingOrganisation` | `string` | Yes | Issuing body. |
| `issuedMonth` | `integer` | Yes | Month issued (1–12). |
| `issuedYear` | `integer` | Yes | Year issued. |
| `expiryYear` | `integer` | Yes | Expiry year (if applicable). |
| `credentialId` | `string` | Yes | Certificate ID or reference number. |
| `credentialUrl` | `string` | Yes | URL to verify credential. |
| `sortOrder` | `integer` | No | Display order. |

---

### Add / Update / Delete certificate

**`POST`** `/api/v1/me/profile/certificates` — Add. Response: `{ id: UUID }`.

**`PUT`** `/api/v1/me/profile/certificates/{id}` — Update.

**`DELETE`** `/api/v1/me/profile/certificates/{id}` — Delete.

---

## Skills

### List skills

**`GET`** `/api/v1/me/profile/skills`

#### Response `200 OK`

Array of `JobSeekerSkillDto`:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Entry ID. |
| `name` | `string` | Yes | Skill name (e.g. "React", "Project Management"). |
| `skillLevel` | `SkillLevel` | Yes | `Beginner`, `Intermediate`, `Advanced`, or `Expert`. |
| `sortOrder` | `integer` | No | Display order. |

---

### Add / Update / Delete skill

**`POST`** `/api/v1/me/profile/skills` — Add. Response: `{ id: UUID }`.

**`PUT`** `/api/v1/me/profile/skills/{id}` — Update.

**`DELETE`** `/api/v1/me/profile/skills/{id}` — Delete.

---

## Resumes

### Link an uploaded file as resume

**`POST`** `/api/v1/me/profile/resume/{fileId}`

Links a previously uploaded file (via [Files](08-files.md)) to the profile as an active resume. The file becomes the default resume if no other default exists.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `fileId` | `UUID` | ID returned from the file upload endpoint. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `id` | `string (UUID)` | ID of the created resume record. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Resume linked. |
| `404 Not Found` | File or profile not found. |

---

### Get resume AI analysis

**`GET`** `/api/v1/me/resumes/{resumeId}/analysis`

Returns the most recent AI analysis result for a specific resume.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `resumeId` | `UUID` | Resume record ID. |

#### Response `200 OK`

Resume analysis data including extracted skills, experience summary, and match suggestions.

---

### Trigger resume AI analysis

**`POST`** `/api/v1/me/resumes/{resumeId}/analysis/trigger`

Queues the resume for AI analysis. Analysis is asynchronous — poll [Get resume AI analysis](#get-resume-ai-analysis) for the result.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `resumeId` | `UUID` | Resume record ID. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Analysis queued. |
| `404 Not Found` | Resume not found. |

---

## Delete my account

**`DELETE`** `/api/v1/me/account`

Permanently deletes the authenticated job seeker's account and all associated data. This action is **irreversible**.

**What is deleted:**

- All applications where the candidate was not hired
- All remaining applications
- Job seeker profile and sub-records (work history, skills, qualifications, certificates, languages, nationalities)
- All uploaded files (resumes, profile picture)
- The user account itself (email address, credentials)

After deletion, the email address is fully removed. Re-registration with the same email creates an entirely new account with no prior history.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `message` | `string` | Confirmation message. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Account and data deleted. |
| `401 Unauthorized` | Not authenticated. |
| `404 Not Found` | Account not found (should not occur for authenticated users). |
