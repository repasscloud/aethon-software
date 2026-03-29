---
title: Recruiter–Company Partnerships
description: Recruiter-side partnership requests with companies
weight: 120
---

# Recruiter–Company Partnerships

All endpoints under `/api/v1/recruiter/companies` require authentication. These endpoints allow recruiter agency members to view their company partnerships, request new partnerships, and cancel pending requests.

A partnership grants the recruiter agency permission to create and submit jobs on behalf of the company. The scope of those permissions is defined when the company approves the request (see [Company–Recruiter Management](13-company-recruiters.md)).

---

## List my company partnerships

**`GET`** `/api/v1/recruiter/companies`

> 🔒 **Authenticated** — recruiter member

Returns all company partnerships (active or pending) for the caller's recruiter organisation.

#### Response `200 OK`

Array of partnership objects:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Partnership ID. |
| `recruiterOrganisationId` | `string (UUID)` | No | Recruiter organisation ID. |
| `companyOrganisationId` | `string (UUID)` | No | Company organisation ID. |
| `recruiterOrganisationName` | `string` | No | Recruiter organisation name. |
| `companyOrganisationName` | `string` | No | Company organisation name. |
| `status` | `OrganisationRecruitmentPartnershipStatus` | No | `Pending`, `Active`, `Suspended`, `Revoked`, or `Rejected`. |
| `scope` | `OrganisationRecruitmentPartnershipScope` | No | Flags enum of permitted operations. |
| `recruiterCanCreateUnclaimedCompanyJobs` | `boolean` | No | Whether recruiter can create jobs for unclaimed companies. |
| `recruiterCanPublishJobs` | `boolean` | No | Whether recruiter can publish jobs directly. |
| `recruiterCanManageCandidates` | `boolean` | No | Whether recruiter can manage candidates. |
| `requestedByUserId` | `string (UUID)` | Yes | User who initiated the request. |
| `approvedByUserId` | `string (UUID)` | Yes | User who approved the request. |
| `createdUtc` | `string (ISO 8601)` | No | When the request was created. |
| `approvedUtc` | `string (ISO 8601)` | Yes | When the request was approved. |
| `notes` | `string` | Yes | Notes from the company on approval. |

---

## Request partnership

**`POST`** `/api/v1/recruiter/companies/requests`

> 🔒 **Authenticated** — recruiter member

Sends a partnership request to a company organisation. The company must review and approve or reject the request.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `companyOrganisationId` | `string (UUID)` | Yes | ID of the company to partner with. |
| `message` | `string` | No | Optional message to include with the request. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `partnershipId` | `string (UUID)` | Newly created partnership request ID. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Partnership request sent. |
| `400 Bad Request` | Validation failed. |
| `409 Conflict` | A partnership with this company already exists. |

---

## Cancel partnership request

**`DELETE`** `/api/v1/recruiter/companies/requests/{partnershipId}`

> 🔒 **Authenticated** — recruiter member

Cancels a pending partnership request. Only requests in `Pending` status can be cancelled.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `partnershipId` | `UUID` | Partnership request ID. |

#### Response `200 OK`

Confirmation that the request was cancelled.

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Request cancelled. |
| `400 Bad Request` | Request is not in `Pending` status. |
| `403 Forbidden` | Caller does not own this request. |
| `404 Not Found` | Partnership request not found. |
