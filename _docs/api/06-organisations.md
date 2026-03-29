---
title: Organisations
description: Organisation profile, team management, domain verification, invites, and company claims
weight: 60
---

# Organisations

All endpoints under `/api/v1/organisations` require authentication unless noted.

These endpoints are used by company owners, admins, and recruiters to manage their organisation's profile, team members, verified domains, and partnership claims.

---

## Profile

### Get organisation profile

**`GET`** `/api/v1/organisations/me/profile`

> 🔒 **Authenticated** — company or recruiter member

Returns the full organisation profile for the caller's active organisation.

#### Response `200 OK`

Organisation profile object including name, description, logo, website, verified status, and settings.

---

### Update organisation profile

**`PUT`** `/api/v1/organisations/me/profile`

> 🔒 **Authenticated** — organisation owner or admin member

Updates the organisation profile. Fields omitted retain their current values.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | `string` | No | Organisation display name. Max 250 chars. |
| `description` | `string` | No | Organisation description. Max 4000 chars. |
| `website` | `string` | No | Website URL. |
| `logoUrl` | `string` | No | Logo image URL. |
| `industry` | `string` | No | Industry sector. |
| `companySize` | `CompanySize` | No | Employee count range. |
| `founded` | `integer` | No | Founding year. |
| `linkedInUrl` | `string` | No | LinkedIn company page URL. |
| `slug` | `string` | No | Public URL slug. Must be unique. |
| `isPublicProfileEnabled` | `boolean` | No | Whether the public organisation page is active. |

#### Response `200 OK`

Updated organisation profile.

---

## Members

### List members

**`GET`** `/api/v1/organisations/me/members`

> 🔒 **Authenticated** — organisation member

Lists all members of the caller's active organisation.

#### Response `200 OK`

Array of member objects:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `userId` | `string (UUID)` | No | User ID. |
| `displayName` | `string` | No | Member display name. |
| `email` | `string` | No | Member email. |
| `companyRole` | `CompanyRole` | Yes | Role within a company org. |
| `recruiterRole` | `RecruiterRole` | Yes | Role within a recruiter org. |
| `status` | `MembershipStatus` | No | `Active`, `Suspended`, or `Invited`. |
| `isOwner` | `boolean` | No | Whether this member is the organisation owner. |
| `joinedUtc` | `string (ISO 8601)` | Yes | When the member joined. |

---

### Get member detail

**`GET`** `/api/v1/organisations/me/members/{userId}`

> 🔒 **Authenticated**

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `userId` | `UUID` | Member user ID. |

#### Response `200 OK`

Full member detail including profile, verification status, and activity summary.

---

### Update member role

**`PUT`** `/api/v1/organisations/me/members/{userId}/role`

> 🔒 **Authenticated** — organisation owner

Changes a member's role within the organisation.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `userId` | `UUID` | Member user ID. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `companyRole` | `CompanyRole` | Cond. | Required for company organisations. |
| `recruiterRole` | `RecruiterRole` | Cond. | Required for recruiter organisations. |

---

### Update member status

**`PUT`** `/api/v1/organisations/me/members/{userId}/status`

> 🔒 **Authenticated** — organisation owner

Activates or suspends a member's access to the organisation.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `status` | `MembershipStatus` | Yes | `Active` or `Suspended`. |

---

### Verify member identity

**`POST`** `/api/v1/organisations/me/members/{userId}/verify-identity`

> 🔒 **Authenticated** — organisation owner or admin

Initiates an identity verification check for a member.

---

### Remove member

**`DELETE`** `/api/v1/organisations/me/members/{userId}`

> 🔒 **Authenticated** — organisation owner

Removes a member from the organisation. The user's account is not deleted.

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Member removed. |
| `400 Bad Request` | Cannot remove the last owner. |
| `404 Not Found` | Member not found. |

---

## Member profile (self)

### Get my membership profile

**`GET`** `/api/v1/organisations/me/my-profile`

> 🔒 **Authenticated**

Returns the caller's own membership profile within their active organisation (bio, title, avatar, etc.).

---

### Update my membership profile

**`PUT`** `/api/v1/organisations/me/my-profile`

> 🔒 **Authenticated**

Updates the caller's own membership profile.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `title` | `string` | No | Job title. Max 200 chars. |
| `bio` | `string` | No | Short biography. Max 1000 chars. |
| `avatarUrl` | `string` | No | Avatar image URL. |
| `slug` | `string` | No | Public profile slug. |
| `linkedInUrl` | `string` | No | LinkedIn profile URL. |
| `isPublicProfileEnabled` | `boolean` | No | Whether the public member profile is visible. |

---

### Update display name

**`PUT`** `/api/v1/organisations/me/display-name`

> 🔒 **Authenticated**

Updates the caller's account-level display name.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `displayName` | `string` | Yes | New display name. Max 200 chars. |

---

## Invitations

### Invite member

**`POST`** `/api/v1/organisations/me/invites`

> 🔒 **Authenticated** — organisation owner or admin

Sends an email invitation to join the organisation.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `email` | `string` | Yes | Email address to invite. |
| `role` | `string` | Yes | Role to assign on acceptance. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `inviteId` | `string (UUID)` | Invitation ID. |
| `message` | `string` | Confirmation. |

---

### Accept invitation

**`POST`** `/api/v1/organisations/invites/accept`

> 🔒 **Authenticated**

Accepts an invitation using the token from the invitation email. The caller's account is added to the organisation.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `token` | `string` | Yes | Invitation token from the email. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Invitation accepted. |
| `400 Bad Request` | Token invalid, expired, or already used. |
| `409 Conflict` | Caller is already a member of this organisation. |

---

## Domains

### List domains

**`GET`** `/api/v1/organisations/me/domains`

> 🔒 **Authenticated**

Lists email domains associated with the organisation.

#### Response `200 OK`

Array of domain objects:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Domain record ID. |
| `domain` | `string` | No | Domain name (e.g. `example.com`). |
| `isPrimary` | `boolean` | No | Whether this is the primary domain. |
| `status` | `DomainStatus` | No | `Pending`, `Verified`, or `Failed`. |
| `verificationMethod` | `DomainVerificationMethod` | No | `DnsTxt` or `None`. |
| `trustLevel` | `DomainTrustLevel` | No | `None`, `Low`, `Medium`, or `High`. |
| `verifiedUtc` | `string (ISO 8601)` | Yes | When the domain was verified. |

---

### Add domain

**`POST`** `/api/v1/organisations/me/domains`

> 🔒 **Authenticated** — organisation owner

Adds a domain to the organisation. DNS TXT verification is initiated automatically.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `domain` | `string` | Yes | Domain to add (e.g. `example.com`). |

---

### Confirm domain verification

**`POST`** `/api/v1/organisations/me/domains/{domainId}/confirm-verification`

> 🔒 **Authenticated**

Manually triggers a verification check for a pending domain. The background worker also runs hourly — this endpoint forces an immediate check.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `domainId` | `UUID` | Domain record ID. |

---

### Regenerate verification token

**`POST`** `/api/v1/organisations/me/domains/{domainId}/regenerate-token`

> 🔒 **Authenticated**

Regenerates the DNS TXT verification token for a domain. Use if the current token has expired or was not correctly deployed.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `domainId` | `UUID` | Domain record ID. |

---

## Slug check

**`GET`** `/api/v1/organisations/check-slug`

> 🔒 **Authenticated**

Checks whether an organisation slug is available before saving.

#### Query parameters

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `slug` | `string` | Yes | Desired slug. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `available` | `boolean` | `true` if the slug is free. |
| `slug` | `string` | Normalised (lowercase) slug. |

---

## Organisation claims

### Search claimable organisations

**`GET`** `/api/v1/organisations/claimable`

> 🔓 **Public**

Returns organisations that have not yet been claimed by a registered user. Used by recruiters and companies to find their organisation before creating a claim.

#### Query parameters

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `search` | `string` | No | Name search term. |

#### Response `200 OK`

Array of claimable organisation summaries.

---

### Submit claim request

**`POST`** `/api/v1/organisations/claim`

> 🔒 **Authenticated**

Submits a request to claim an existing unclaimed organisation. A claim request is reviewed by the platform team or by the organisation owner if one exists.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `organisationId` | `string (UUID)` | Yes | ID of the organisation to claim. |
| `message` | `string` | No | Supporting message for the claim. |

---

### List my claim requests

**`GET`** `/api/v1/organisations/me/claim-requests`

> 🔒 **Authenticated**

Returns all claim requests submitted by the caller.

---

### Cancel claim request

**`DELETE`** `/api/v1/organisations/me/claim-requests/{claimId}`

> 🔒 **Authenticated**

Cancels a pending claim request.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `claimId` | `UUID` | Claim request ID. |
