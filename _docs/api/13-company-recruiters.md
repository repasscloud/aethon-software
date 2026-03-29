---
title: Company–Recruiter Management
description: Company-side management of recruiter agency partnerships and permissions
weight: 130
---

# Company–Recruiter Management

All endpoints under `/api/v1/company/recruiters` require authentication. These endpoints allow company members to manage their recruiter agency partnerships — reviewing pending requests, inviting agencies, approving with granular permissions, rejecting, and suspending.

---

## List active recruiters

**`GET`** `/api/v1/company/recruiters`

> 🔒 **Authenticated** — company member

Returns all approved recruiter agency partnerships for the caller's active company organisation.

#### Response `200 OK`

Array of partnership objects. See [Recruiter–Company Partnerships](12-recruiter-companies.md) for field descriptions — same shape returned here from the company perspective.

---

## List pending recruiter requests

**`GET`** `/api/v1/company/recruiters/pending`

> 🔒 **Authenticated** — company member

Returns all partnership requests in `Pending` status awaiting the company's review.

#### Response `200 OK`

Array of pending partnership objects (same shape as above).

---

## Invite recruiter agency

**`POST`** `/api/v1/company/recruiters/invite`

> 🔒 **Authenticated** — company member

Sends an invitation to a recruiter agency, creating a pre-approved partnership with defined permissions. This is the company-initiated flow (vs. the recruiter requesting).

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `recruiterOrganisationId` | `string (UUID)` | Yes | ID of the recruiter agency to invite. |
| `allowCreateDraftJobs` | `boolean` | Yes | Recruiter can create draft jobs. |
| `allowSubmitJobsForApproval` | `boolean` | Yes | Recruiter can submit jobs for company approval. |
| `allowManageApprovedJobs` | `boolean` | Yes | Recruiter can edit approved jobs. |
| `allowViewCandidates` | `boolean` | Yes | Recruiter can view candidate profiles. |
| `allowSubmitCandidates` | `boolean` | Yes | Recruiter can submit candidates for jobs. |
| `allowCommunicateWithCandidates` | `boolean` | Yes | Recruiter can communicate with candidates. |
| `allowScheduleInterviews` | `boolean` | Yes | Recruiter can schedule interviews. |
| `allowPublishJobs` | `boolean` | Yes | Recruiter can publish jobs directly. |
| `recruiterCanCreateUnclaimedCompanyJobs` | `boolean` | Yes | Recruiter can create jobs for unclaimed companies. |
| `recruiterCanPublishJobs` | `boolean` | Yes | Recruiter can publish jobs without approval. |
| `recruiterCanManageCandidates` | `boolean` | Yes | Recruiter can manage candidates independently. |
| `message` | `string` | No | Optional invitation message. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `partnershipId` | `string (UUID)` | Created partnership ID. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Invitation sent and partnership created. |
| `400 Bad Request` | Validation failed or recruiter not found. |
| `409 Conflict` | Partnership already exists with this agency. |

---

## Approve recruiter request

**`POST`** `/api/v1/company/recruiters/{partnershipId}/approve`

> 🔒 **Authenticated** — company member

Approves a pending partnership request and sets the permissions for the recruiter agency.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `partnershipId` | `UUID` | Partnership request ID. |

#### Request body

Same permission fields as [Invite recruiter agency](#invite-recruiter-agency) above, plus:

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `notes` | `string` | No | Internal notes about the approval. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `partnershipId` | `string (UUID)` | Partnership ID. |
| `status` | `string` | New status: `Active`. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Request approved. |
| `400 Bad Request` | Request is not in `Pending` status. |
| `404 Not Found` | Partnership not found. |

---

## Reject recruiter request

**`POST`** `/api/v1/company/recruiters/{partnershipId}/reject`

> 🔒 **Authenticated** — company member

Rejects a pending partnership request.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `partnershipId` | `UUID` | Partnership request ID. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `reason` | `string` | No | Reason for rejection (visible to the recruiter). |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `partnershipId` | `string (UUID)` | Partnership ID. |
| `status` | `string` | New status: `Rejected`. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Request rejected. |
| `400 Bad Request` | Request is not in `Pending` status. |
| `404 Not Found` | Partnership not found. |

---

## Suspend recruiter

**`POST`** `/api/v1/company/recruiters/{partnershipId}/suspend`

> 🔒 **Authenticated** — company member

Suspends an active recruiter partnership, preventing the agency from creating or submitting jobs until reactivated.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `partnershipId` | `UUID` | Partnership ID. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `partnershipId` | `string (UUID)` | Partnership ID. |
| `status` | `string` | New status: `Suspended`. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Partnership suspended. |
| `400 Bad Request` | Partnership is not in `Active` status. |
| `404 Not Found` | Partnership not found. |
