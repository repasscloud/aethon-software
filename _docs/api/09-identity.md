---
title: Identity Verification
description: Submit and track identity verification requests for job seekers
weight: 90
---

# Identity Verification

Endpoints under `/api/v1/identity` allow job seekers to submit identity verification requests and track their status.

Once verified by an administrator, `IsIdVerified = true` is set on the job seeker profile and the name fields become locked (cannot be changed without admin intervention).

---

## Submit verification request

**`POST`** `/api/v1/identity/verification-request`

> 🔒 **Authenticated** — job seeker

Submits an identity verification request. The request is reviewed by platform staff.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `documentType` | `string` | Yes | Type of identity document provided (e.g. `Passport`, `DriversLicence`, `NationalId`). |
| `documentReference` | `string` | No | Reference number or identifier on the document. |
| `notes` | `string` | No | Additional notes for the reviewer. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `id` | `string (UUID)` | Verification request ID. |
| `status` | `VerificationRequestStatus` | Initial status: `Pending`. |
| `submittedUtc` | `string (ISO 8601)` | Submission timestamp. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Request submitted. |
| `409 Conflict` | A pending or approved request already exists. |

---

## Get my verification request

**`GET`** `/api/v1/identity/verification-request/mine`

> 🔒 **Authenticated** — job seeker

Returns the caller's most recent identity verification request, if any.

#### Response `200 OK`

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Request ID. |
| `status` | `VerificationRequestStatus` | No | `Pending`, `Approved`, `Denied`, or `Expired`. |
| `documentType` | `string` | Yes | Document type submitted. |
| `submittedUtc` | `string (ISO 8601)` | No | When submitted. |
| `reviewedUtc` | `string (ISO 8601)` | Yes | When reviewed. |
| `reviewerNotes` | `string` | Yes | Notes from the reviewer (visible to the submitter). |

Returns `null` (with `200 OK`) if no request has been submitted.
