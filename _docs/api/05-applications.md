---
title: Applications
description: Submit, view, and manage job applications. Covers both candidate and employer perspectives.
weight: 50
---

# Applications

All endpoints under `/api/v1/applications` require authentication.

Candidates use these endpoints to track their applications and withdraw them. Employers and recruiters use them to review, move through workflow stages, schedule interviews, attach files, and leave internal notes.

The same application object is returned for both perspectives — access to sensitive fields is governed server-side by the caller's role.

---

## Get status options

**`GET`** `/api/v1/applications/status-options`

> 🔒 **Authenticated**

Returns the list of valid `ApplicationStatus` string values. Useful for populating filter dropdowns.

#### Response `200 OK`

Array of status strings: `Draft`, `Submitted`, `UnderReview`, `Shortlisted`, `Interview`, `Offer`, `Hired`, `Rejected`, `Withdrawn`.

---

## Submit application

**`POST`** `/api/v1/applications`

> 🔒 **Authenticated** — job seeker only

Submits a job application. Validates age group eligibility before accepting.

Prefer the public endpoint [`POST /api/v1/public/jobs/{jobId}/apply`](02-public.md#apply-to-a-job-in-platform) for applications submitted from the job board. This internal endpoint follows the same logic.

---

## List my applications

**`GET`** `/api/v1/applications/mine`

> 🔒 **Authenticated** — job seeker only

Returns the authenticated job seeker's own applications, ordered by most recently submitted.

#### Query parameters

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `page` | `integer` | No | `1` | Page number. |
| `pageSize` | `integer` | No | `20` | Items per page. |

#### Response `200 OK`

Paginated list of `JobApplicationListItemDto`:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Application ID. |
| `jobId` | `string (UUID)` | No | Job ID. |
| `jobTitle` | `string` | No | Job title at time of application. |
| `organisationName` | `string` | No | Employer name. |
| `status` | `ApplicationStatus` | No | Current workflow status. |
| `submittedUtc` | `string (ISO 8601)` | No | When the application was submitted. |
| `lastStatusChangedUtc` | `string (ISO 8601)` | Yes | When the status last changed. |

---

## Get application

**`GET`** `/api/v1/applications/{applicationId}`

> 🔒 **Authenticated**

Returns full details of a single application. Candidates can only access their own applications. Employers and recruiters can access applications for jobs they manage.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `applicationId` | `UUID` | Application identifier. |

#### Response `200 OK`

`EmployerJobApplicationDetailDto` — see [Data Schemas](../models/schemas.md#employerjobapplicationdetaildto).

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Application found. |
| `403 Forbidden` | Caller does not have access to this application. |
| `404 Not Found` | Application not found. |

---

## Get application timeline

**`GET`** `/api/v1/applications/{applicationId}/timeline`

> 🔒 **Authenticated**

Returns the full audit trail of status changes, notes, and activity for an application.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `applicationId` | `UUID` | Application identifier. |

#### Response `200 OK`

Array of timeline event objects ordered by timestamp ascending:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Event ID. |
| `eventType` | `string` | No | Type of event (e.g. `StatusChange`, `Note`, `Comment`, `Interview`). |
| `description` | `string` | Yes | Human-readable event description. |
| `createdUtc` | `string (ISO 8601)` | No | When the event occurred. |
| `createdByUserId` | `string (UUID)` | Yes | User who created the event. |
| `createdByName` | `string` | Yes | Display name of the user. |

---

## Change application status

**`POST`** `/api/v1/applications/{applicationId}/status`

> 🔒 **Authenticated** — employer/recruiter only

Moves an application to a new status. Status transitions follow a workflow — invalid transitions are rejected with `422`.

Valid transitions:

```
Submitted → UnderReview → Shortlisted → Interview → Offer → Hired
                                                           → Rejected
          → Rejected
Submitted → Withdrawn (candidate-initiated; use this endpoint for admin-initiated withdrawal)
```

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `applicationId` | `UUID` | Application identifier. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `status` | `ApplicationStatus` | Yes | Target status. |
| `reason` | `string` | No | Reason for the status change (especially for `Rejected`). |
| `notes` | `string` | No | Internal notes recorded alongside the transition. |

#### Response `200 OK`

Updated application detail.

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Status changed. |
| `403 Forbidden` | Caller cannot modify this application. |
| `404 Not Found` | Application not found. |
| `422 Unprocessable Entity` | Invalid status transition. |

---

## Add note

**`POST`** `/api/v1/applications/{applicationId}/notes`

> 🔒 **Authenticated** — employer/recruiter only

Adds a private internal note to the application. Notes are visible to all organisation members but not to the candidate.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `applicationId` | `UUID` | Application identifier. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `content` | `string` | Yes | Note text. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `id` | `string (UUID)` | Note ID. |
| `content` | `string` | Note content. |
| `createdUtc` | `string (ISO 8601)` | When the note was created. |
| `createdByName` | `string` | Display name of the note author. |

---

## Add comment

**`POST`** `/api/v1/applications/{applicationId}/comments`

> 🔒 **Authenticated** — employer/recruiter only

Adds a threaded comment to the application for team discussion. Supports replies via `parentCommentId`.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `applicationId` | `UUID` | Application identifier. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `content` | `string` | Yes | Comment text. |
| `parentCommentId` | `string (UUID)` | No | ID of the comment being replied to. |

#### Response `200 OK`

Comment object including `id`, `content`, `parentCommentId`, `createdUtc`, `createdByName`.

---

## Schedule interview

**`POST`** `/api/v1/applications/{applicationId}/interviews`

> 🔒 **Authenticated** — employer/recruiter only

Records a scheduled interview against an application. Does not send calendar invites — use your own calendar integration for that.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `applicationId` | `UUID` | Application identifier. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `type` | `InterviewType` | Yes | `Phone`, `Video`, `InPerson`, `Technical`, or `Panel`. |
| `title` | `string` | No | Interview title/description. |
| `location` | `string` | No | Physical location (for in-person). |
| `meetingUrl` | `string` | No | Video call URL. |
| `notes` | `string` | No | Internal notes for the interview. |
| `scheduledStartUtc` | `string (ISO 8601)` | Yes | Interview start time (UTC). |
| `scheduledEndUtc` | `string (ISO 8601)` | Yes | Interview end time (UTC). |

#### Response `200 OK`

Interview object with `id`, `type`, `title`, `scheduledStartUtc`, `scheduledEndUtc`, `status`.

---

## Attach file

**`POST`** `/api/v1/applications/{applicationId}/files`

> 🔒 **Authenticated** — employer/recruiter only

Attaches a previously uploaded file to an application (e.g. a skills assessment, reference, or compliance document).

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `applicationId` | `UUID` | Application identifier. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `storedFileId` | `string (UUID)` | Yes | ID of an already-uploaded file. |
| `category` | `string` | Yes | File category label (e.g. `Assessment`, `Reference`, `Compliance`). |
| `notes` | `string` | No | Notes about the attachment. |

#### Response `200 OK`

Attachment record with `id`, `storedFileId`, `category`, `notes`, `attachedUtc`.

---

## List application files

**`GET`** `/api/v1/applications/{applicationId}/files`

> 🔒 **Authenticated**

Returns all files attached to an application.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `applicationId` | `UUID` | Application identifier. |

#### Response `200 OK`

Array of attachment records:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Attachment record ID. |
| `storedFileId` | `string (UUID)` | No | Stored file ID (use to download). |
| `originalFileName` | `string` | No | Original filename. |
| `contentType` | `string` | No | MIME type. |
| `lengthBytes` | `integer` | No | File size in bytes. |
| `category` | `string` | Yes | Category label. |
| `notes` | `string` | Yes | Attachment notes. |
| `attachedUtc` | `string (ISO 8601)` | No | When attached. |
