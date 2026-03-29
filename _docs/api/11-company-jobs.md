---
title: Company Job Approvals
description: Company-side approval workflow for recruiter-submitted jobs
weight: 110
---

# Company Job Approvals

All endpoints under `/api/v1/company/jobs/approvals` require authentication. These endpoints are used by company members to review, approve, or reject jobs that recruiter agencies have submitted on their behalf.

When a recruiter submits a job for approval, it appears in the pending approvals queue. The company can then approve it (moving it to `Approved` status, ready to publish) or reject it (returning it to the recruiter with a reason).

---

## List pending approvals

**`GET`** `/api/v1/company/jobs/approvals`

> 🔒 **Authenticated** — company member

Returns all jobs currently in `PendingCompanyApproval` status for the caller's active company organisation.

#### Response `200 OK`

Array of pending job approval objects:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Job ID. |
| `title` | `string` | No | Job title. |
| `locationText` | `string` | Yes | Location display text. |
| `employmentType` | `EmploymentType` | No | Employment type. |
| `workplaceType` | `WorkplaceType` | No | Workplace arrangement. |
| `status` | `JobStatus` | No | Always `PendingCompanyApproval`. |
| `recruiterOrganisationName` | `string` | Yes | Name of the recruiter agency that submitted the job. |
| `createdUtc` | `string (ISO 8601)` | No | When the job was created. |

---

## Approve job

**`POST`** `/api/v1/company/jobs/approvals/{jobId}/approve`

> 🔒 **Authenticated** — company member

Approves a recruiter-submitted job. The job moves to `Approved` status and can then be published via the billing flow.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job ID. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `jobId` | `string (UUID)` | Job ID. |
| `status` | `JobStatus` | New status: `Approved`. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Job approved. |
| `400 Bad Request` | Job is not in `PendingCompanyApproval` status. |
| `403 Forbidden` | Caller is not a member of the owning company. |
| `404 Not Found` | Job not found. |

---

## Reject job

**`POST`** `/api/v1/company/jobs/approvals/{jobId}/reject`

> 🔒 **Authenticated** — company member

Rejects a recruiter-submitted job with an optional reason. The job is returned to `Draft` status so the recruiter can make changes and resubmit.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job ID. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `reason` | `string` | No | Reason for rejection (visible to the recruiter). |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `jobId` | `string (UUID)` | Job ID. |
| `status` | `JobStatus` | New status: `Draft`. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Job rejected and returned to draft. |
| `400 Bad Request` | Job is not in `PendingCompanyApproval` status. |
| `403 Forbidden` | Caller is not a member of the owning company. |
| `404 Not Found` | Job not found. |
