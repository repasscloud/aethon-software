---
title: Recruiter Jobs
description: Recruiter job draft creation, updating, and submission for company approval
weight: 100
---

# Recruiter Jobs

All endpoints under `/api/v1/recruiter/jobs` require authentication. These endpoints are used by recruiter agency members to create job drafts on behalf of a partner company and submit them for approval.

A recruiter must have an active partnership with the target company before creating jobs for them. Once submitted, the job enters a `PendingCompanyApproval` status and awaits review by the company.

---

## List recruiter jobs

**`GET`** `/api/v1/recruiter/jobs`

> 🔒 **Authenticated** — recruiter member

Returns all jobs created by or associated with the caller's recruiter organisation.

#### Response `200 OK`

Array of `JobListItemDto`:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Job ID. |
| `title` | `string` | No | Job title. |
| `department` | `string` | Yes | Department or team. |
| `locationText` | `string` | Yes | Location display string. |
| `workplaceType` | `WorkplaceType` | No | `OnSite`, `Hybrid`, or `Remote`. |
| `employmentType` | `EmploymentType` | No | Employment type. |
| `status` | `JobStatus` | No | Current job status. |
| `salaryFrom` | `number` | Yes | Minimum salary. |
| `salaryTo` | `number` | Yes | Maximum salary. |
| `salaryCurrency` | `CurrencyCode` | Yes | Salary currency. |
| `createdUtc` | `string (ISO 8601)` | No | When the job was created. |
| `publishedUtc` | `string (ISO 8601)` | Yes | When the job was published. |

---

## Create job draft

**`POST`** `/api/v1/recruiter/jobs`

> 🔒 **Authenticated** — recruiter member

Creates a new job draft on behalf of a partner company. The recruiter's organisation must have an active partnership with the specified company.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `companyOrganisationId` | `string (UUID)` | Yes | ID of the partner company this job is for. |
| `title` | `string` | Yes | Job title. |
| `summary` | `string` | No | Short job summary. |
| `description` | `string` | No | Full job description. |
| `location` | `string` | No | Location display text. |
| `salaryMin` | `number` | No | Minimum salary. |
| `salaryMax` | `number` | No | Maximum salary. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `id` | `string (UUID)` | Newly created job ID. |
| `status` | `JobStatus` | Initial status: `Draft`. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Draft created. |
| `400 Bad Request` | Validation failed or no active partnership with the specified company. |

---

## Update job draft

**`PUT`** `/api/v1/recruiter/jobs/{jobId}`

> 🔒 **Authenticated** — recruiter member

Updates an existing job draft. The job must be in `Draft` status and belong to the caller's organisation.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job ID. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `title` | `string` | Yes | Job title. |
| `summary` | `string` | No | Short job summary. |
| `description` | `string` | No | Full job description. |
| `location` | `string` | No | Location display text. |
| `salaryMin` | `number` | No | Minimum salary. |
| `salaryMax` | `number` | No | Maximum salary. |

#### Response `200 OK`

Updated job object.

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Draft updated. |
| `400 Bad Request` | Validation failed or job not in a state that allows editing. |
| `403 Forbidden` | Caller does not own this job. |
| `404 Not Found` | Job not found. |

---

## Submit job for approval

**`POST`** `/api/v1/recruiter/jobs/{jobId}/submit`

> 🔒 **Authenticated** — recruiter member

Submits a job draft to the partner company for approval. The job status changes from `Draft` to `PendingCompanyApproval`. The company can then approve or reject the submission.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job ID. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `jobId` | `string (UUID)` | Job ID. |
| `status` | `JobStatus` | New status: `PendingCompanyApproval`. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Job submitted for approval. |
| `400 Bad Request` | Job is not in `Draft` status or other validation error. |
| `403 Forbidden` | Caller does not own this job. |
| `404 Not Found` | Job not found. |
