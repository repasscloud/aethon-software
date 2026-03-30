---
title: Admin
description: Platform administration endpoints for staff — organisations, users, jobs, billing, settings, logs, and maintenance
weight: 150
---

# Admin

All endpoints under `/api/v1/admin` require an active staff session. The group requires one of the roles `SuperAdmin`, `Admin`, or `Support`. Many individual endpoints further restrict access to `SuperAdmin` or `Admin` only — these are noted inline.

> 🛡️ **Admin** — requires `SuperAdmin`, `Admin`, or `Support` role
> 🛡️⬆️ **SuperAdmin/Admin** — requires `SuperAdmin` or `Admin` role
> 🛡️⬆️⬆️ **SuperAdmin only** — requires `SuperAdmin` role

---

## Stats

### Get platform stats

**`GET`** `/api/v1/admin/stats`

> 🛡️ **Admin**

Returns high-level platform counts for the admin dashboard.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `organisations` | `integer` | Total organisation count. |
| `jobs` | `integer` | Total job count. |
| `users` | `integer` | Total user count. |
| `files` | `integer` | Total stored file count. |
| `verifiedOrganisations` | `integer` | Organisations with a non-`None` verification tier. |
| `pendingStripeEvents` | `integer` | Stripe payment events in `Pending` status. |
| `emailConfigured` | `boolean` | Whether outbound email is configured. |

---

## Organisations

### List organisations

**`GET`** `/api/v1/admin/organisations`

> 🛡️ **Admin**

Returns a paginated list of all organisations.

#### Query parameters

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `search` | `string` | No | — | Name or slug search. |
| `page` | `integer` | No | `1` | Page number. |

Page size is fixed at 50.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `total` | `integer` | Total matching organisations. |
| `page` | `integer` | Current page. |
| `pageSize` | `integer` | Items per page (50). |
| `items` | `array` | Organisation summaries. |

Each item:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Organisation ID. |
| `name` | `string` | No | Organisation name. |
| `slug` | `string` | Yes | Public slug. |
| `type` | `OrganisationType` | No | `Company` or `RecruiterAgency`. |
| `status` | `OrganisationStatus` | No | Lifecycle status. |
| `verificationTier` | `VerificationTier` | No | `None`, `StandardEmployer`, or `EnhancedTrusted`. |
| `verifiedUtc` | `string (ISO 8601)` | Yes | When verified. |
| `isPublicProfileEnabled` | `boolean` | No | Whether public profile is visible. |
| `createdUtc` | `string (ISO 8601)` | No | Creation date. |
| `jobCount` | `integer` | No | Number of jobs owned by this org. |

---

### Get organisation detail

**`GET`** `/api/v1/admin/organisations/{orgId}`

> 🛡️ **Admin**

Returns full details for a single organisation.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `orgId` | `UUID` | Organisation ID. |

#### Response `200 OK`

Extended organisation object including legal name, contact details, claim status, member count, and all verification fields.

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Organisation found. |
| `404 Not Found` | Organisation not found. |

---

### Set verification tier

**`PUT`** `/api/v1/admin/organisations/{orgId}/verification`

> 🛡️⬆️ **SuperAdmin/Admin**

Sets the verification tier for an organisation. When upgrading from `None` to a verified tier, promo Standard credits are automatically converted to Premium (if the `Feature.VerificationUpgradesPromoCredits` setting is enabled).

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `orgId` | `UUID` | Organisation ID. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `tier` | `VerificationTier` | Yes | `None`, `StandardEmployer`, or `EnhancedTrusted`. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `organisationId` | `string (UUID)` | Organisation ID. |
| `tier` | `string` | Applied tier name. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Tier updated. |
| `404 Not Found` | Organisation not found. |

---

### List partnerships

**`GET`** `/api/v1/admin/organisations/{orgId}/partnerships`

> 🛡️ **Admin**

Returns all recruiter–company partnerships involving the specified organisation (either as the company or the recruiter).

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `orgId` | `UUID` | Organisation ID. |

#### Response `200 OK`

Array of partnership objects. Fields match `RecruiterCompanyRelationshipDto` — see [Recruiter–Company Partnerships](12-recruiter-companies.md#list-my-company-partnerships).

---

### Update partnership status

**`PUT`** `/api/v1/admin/organisations/{orgId}/partnerships/{pid}/status`

> 🛡️ **Admin**

Updates the status of a specific partnership.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `orgId` | `UUID` | Organisation ID (company or recruiter side). |
| `pid` | `UUID` | Partnership ID. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `status` | `OrganisationRecruitmentPartnershipStatus` | Yes | New status. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `partnershipId` | `string (UUID)` | Partnership ID. |
| `status` | `string` | Applied status. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Status updated. |
| `404 Not Found` | Partnership not found. |

---

### Create partnership

**`POST`** `/api/v1/admin/organisations/{orgId}/partnerships`

> 🛡️⬆️ **SuperAdmin/Admin**

Manually creates a recruiter–company partnership, bypassing the request/approval flow. `{orgId}` is the company organisation.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `orgId` | `UUID` | Company organisation ID. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `recruiterOrganisationId` | `string (UUID)` | Yes | Recruiter organisation ID. |
| `status` | `OrganisationRecruitmentPartnershipStatus` | No | Initial status. Defaults to `Active`. |
| `scope` | `OrganisationRecruitmentPartnershipScope` | No | Scope flags. |
| `recruiterCanCreateUnclaimedCompanyJobs` | `boolean` | No | Permission flag. |
| `recruiterCanPublishJobs` | `boolean` | No | Permission flag. |
| `recruiterCanManageCandidates` | `boolean` | No | Permission flag. |
| `notes` | `string` | No | Internal notes. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `partnershipId` | `string (UUID)` | Created partnership ID. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Partnership created. |
| `404 Not Found` | Company or recruiter organisation not found. |
| `409 Conflict` | Partnership already exists. |

---

### Delete partnership

**`DELETE`** `/api/v1/admin/organisations/{orgId}/partnerships/{pid}`

> 🛡️ **Admin**

Permanently deletes a partnership record.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `orgId` | `UUID` | Organisation ID. |
| `pid` | `UUID` | Partnership ID. |

#### Response `204 No Content`

#### Status codes

| Status | Meaning |
| --- | --- |
| `204 No Content` | Deleted. |
| `404 Not Found` | Partnership not found. |

---

### Get organisation credits

**`GET`** `/api/v1/admin/organisations/{orgId}/credits`

> 🛡️ **Admin**

Returns all job posting credit batches for an organisation, including expired and fully-used batches.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `orgId` | `UUID` | Organisation ID. |

#### Response `200 OK`

Array of credit records:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Credit batch ID. |
| `creditType` | `CreditType` | No | Type of credit. |
| `source` | `CreditSource` | No | How credits were acquired. |
| `quantityOriginal` | `integer` | No | Original quantity. |
| `quantityRemaining` | `integer` | No | Remaining quantity. |
| `expiresAt` | `string (ISO 8601)` | Yes | Expiry date. |
| `convertedAt` | `string (ISO 8601)` | Yes | When promo credits were converted to premium. |
| `grantedByUserId` | `string (UUID)` | Yes | Admin who granted the credits. |
| `grantNote` | `string` | Yes | Note from the granting admin. |
| `stripePaymentEventId` | `string (UUID)` | Yes | Linked Stripe event if purchased. |
| `createdUtc` | `string (ISO 8601)` | No | When the batch was created. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Credits returned. |
| `404 Not Found` | Organisation not found. |

---

### Grant credits

**`POST`** `/api/v1/admin/organisations/{orgId}/credits/grant`

> 🛡️⬆️ **SuperAdmin/Admin**

Manually grants job posting credits to an organisation (e.g. for compensation, onboarding, or adjustments).

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `orgId` | `UUID` | Organisation ID. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `creditType` | `CreditType` | Yes | Type of credit to grant. |
| `quantity` | `integer` | Yes | Number of credits to grant. Must be > 0. |
| `expiresAt` | `string (ISO 8601)` | No | Optional expiry date. |
| `note` | `string` | No | Internal note explaining the grant. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `creditId` | `string (UUID)` | Newly created credit batch ID. |
| `organisationId` | `string (UUID)` | Organisation ID. |
| `creditType` | `string` | Credit type name. |
| `quantity` | `integer` | Credits granted. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Credits granted. |
| `400 Bad Request` | Quantity is zero or negative. |
| `404 Not Found` | Organisation not found. |

---

## Jobs

### List jobs

**`GET`** `/api/v1/admin/jobs`

> 🛡️ **Admin**

Returns a paginated list of all jobs across the platform.

#### Query parameters

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `search` | `string` | No | — | Title search (case-insensitive). |
| `status` | `JobStatus` | No | — | Filter by job status. |
| `page` | `integer` | No | `1` | Page number. |

Page size is fixed at 50.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `total` | `integer` | Total matching jobs. |
| `page` | `integer` | Current page. |
| `pageSize` | `integer` | Items per page (50). |
| `items` | `array` | Job summaries. |

Each item includes: `id`, `title`, `status`, `visibility`, `category`, `locationText`, `employmentType`, `workplaceType`, `publishedUtc`, `createdUtc`, `applyByUtc`, `organisationId`, `organisationName`, `applicationCount`.

---

### Get job detail

**`GET`** `/api/v1/admin/jobs/{jobId}`

> 🛡️ **Admin**

Returns full details of a job including all fields, salary, description, and application count.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `jobId` | `UUID` | Job ID. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Job found. |
| `404 Not Found` | Job not found. |

---

## Users

### List users

**`GET`** `/api/v1/admin/users`

> 🛡️ **Admin**

Returns a paginated list of all platform users.

#### Query parameters

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `search` | `string` | No | — | Email or display name search. |
| `page` | `integer` | No | `1` | Page number. |

Page size is fixed at 50.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `total` | `integer` | Total matching users. |
| `page` | `integer` | Current page. |
| `pageSize` | `integer` | Items per page (50). |
| `items` | `array` | User summaries. |

Each item: `id`, `email`, `displayName`, `userType`, `isEnabled`, `isIdentityVerified`, `organisationName` (first active membership).

---

### Get user detail

**`GET`** `/api/v1/admin/users/{userId}`

> 🛡️ **Admin**

Returns full user detail including roles, memberships, and identity verification status.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `userId` | `UUID` | User ID. |

#### Response `200 OK`

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | User ID. |
| `email` | `string` | No | Email address. |
| `displayName` | `string` | No | Display name. |
| `userType` | `UserAccountType` | No | Account type. |
| `isEnabled` | `boolean` | No | Whether the account is active. |
| `isIdentityVerified` | `boolean` | No | Whether identity has been verified. |
| `identityVerifiedUtc` | `string (ISO 8601)` | Yes | When identity was verified. |
| `identityVerificationNotes` | `string` | Yes | Notes from the verification process. |
| `isPhoneNumberVerified` | `boolean` | No | Phone verification status. |
| `phoneNumberConfirmed` | `boolean` | No | ASP.NET Identity phone confirmation flag. |
| `emailConfirmed` | `boolean` | No | Email confirmation status. |
| `lockoutEnd` | `string (ISO 8601)` | Yes | Lockout end time if locked out. |
| `accessFailedCount` | `integer` | No | Number of consecutive failed logins. |
| `roles` | `string[]` | No | ASP.NET Identity roles. |
| `memberships` | `array` | No | Organisation memberships. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | User found. |
| `404 Not Found` | User not found. |

---

### Create staff user

**`POST`** `/api/v1/admin/users`

> 🛡️⬆️⬆️ **SuperAdmin only**

Creates a new `Admin` or `Support` staff account. Sends a welcome email with a temporary password. The user is required to change their password on first login.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `email` | `string` | Yes | Email address for the new account. |
| `displayName` | `string` | Yes | Display name. |
| `password` | `string` | Yes | Temporary password (must meet complexity requirements). |
| `role` | `string` | Yes | `Admin` or `Support`. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `userId` | `string (UUID)` | New user ID. |
| `email` | `string` | Email address. |
| `role` | `string` | Assigned role. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | User created. |
| `400 Bad Request` | Invalid role or identity errors (password policy violation, duplicate email). |

---

### Set user status

**`PUT`** `/api/v1/admin/users/{userId}/status`

> 🛡️⬆️ **SuperAdmin/Admin**

Enables or disables a user account.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `userId` | `UUID` | User ID. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `isEnabled` | `boolean` | Yes | `true` to enable, `false` to disable. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `userId` | `string (UUID)` | User ID. |
| `isEnabled` | `boolean` | Applied status. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Status updated. |
| `404 Not Found` | User not found. |

---

### Reset user password

**`POST`** `/api/v1/admin/users/{userId}/reset-password`

> 🛡️ **Admin**

Resets a user's password to a new value. Forces the user to be logged out of existing sessions.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `userId` | `UUID` | User ID. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `newPassword` | `string` | Yes | New password (must meet complexity requirements). |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `userId` | `string (UUID)` | User ID. |
| `message` | `string` | Confirmation message. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Password reset. |
| `400 Bad Request` | Password does not meet complexity requirements. |
| `404 Not Found` | User not found. |

---

### Verify user email

**`POST`** `/api/v1/admin/users/{userId}/verify-email`

> 🛡️ **Admin**

Admin-confirms a user's email address without the user needing to click a verification link.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `userId` | `UUID` | User ID. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `userId` | `string (UUID)` | User ID. |
| `emailConfirmed` | `boolean` | Always `true`. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Email confirmed. |
| `400 Bad Request` | Failed to confirm. |
| `404 Not Found` | User not found. |

---

### Verify user identity

**`POST`** `/api/v1/admin/users/{userId}/verify-identity`

> 🛡️ **Admin**

Marks a user's identity as verified, with optional notes.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `userId` | `UUID` | User ID. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `notes` | `string` | No | Notes about how identity was verified. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `userId` | `string (UUID)` | User ID. |
| `identityVerified` | `boolean` | Always `true`. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Identity verified. |
| `404 Not Found` | User not found. |

---

### Force password change

**`POST`** `/api/v1/admin/users/{userId}/force-password-change`

> 🛡️ **Admin**

Flags the user's account so that they are required to set a new password on their next login.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `userId` | `UUID` | User ID. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `userId` | `string (UUID)` | User ID. |
| `mustChangePassword` | `boolean` | Always `true`. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Flag set. |
| `404 Not Found` | User not found. |

---

### Force MFA setup

**`POST`** `/api/v1/admin/users/{userId}/force-mfa-setup`

> 🛡️ **Admin**

Flags the user's account so that they are required to enroll in multi-factor authentication on their next login.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `userId` | `UUID` | User ID. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `userId` | `string (UUID)` | User ID. |
| `mustEnableMfa` | `boolean` | Always `true`. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Flag set. |
| `404 Not Found` | User not found. |

---

## Identity Verification Requests

### List verification requests

**`GET`** `/api/v1/admin/verification-requests`

> 🛡️ **Admin**

Returns a paginated list of identity verification requests submitted by job seekers.

#### Query parameters

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `status` | `string` | No | — | Filter by status: `Pending`, `Approved`, or `Denied`. |
| `page` | `integer` | No | `1` | Page number. |

Page size is fixed at 50.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `total` | `integer` | Total matching requests. |
| `page` | `integer` | Current page. |
| `pageSize` | `integer` | Items per page (50). |
| `items` | `array` | Verification request summaries. |

Each item:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Request ID. |
| `userId` | `string (UUID)` | No | Submitting user ID. |
| `userDisplayName` | `string` | No | Submitter display name. |
| `userEmail` | `string` | No | Submitter email. |
| `fullName` | `string` | Yes | Full name as submitted. |
| `emailAddress` | `string` | Yes | Contact email on request. |
| `phoneNumber` | `string` | Yes | Contact phone on request. |
| `additionalNotes` | `string` | Yes | Notes from the submitter. |
| `status` | `string` | No | `Pending`, `Approved`, or `Denied`. |
| `requestedUtc` | `string (ISO 8601)` | No | When submitted. |
| `reviewedUtc` | `string (ISO 8601)` | Yes | When reviewed. |
| `reviewNotes` | `string` | Yes | Reviewer notes. |
| `reviewerType` | `string` | Yes | `System`, `OrgOwner`, or `Admin`. |

---

### Approve verification request

**`POST`** `/api/v1/admin/verification-requests/{id}/approve`

> 🛡️ **Admin**

Approves a pending identity verification request. Sets `IsIdentityVerified = true` on the user's account.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `id` | `UUID` | Verification request ID. |

#### Request body (optional)

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `notes` | `string` | No | Internal reviewer notes. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `id` | `string (UUID)` | Request ID. |
| `approved` | `boolean` | Always `true`. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Approved. |
| `400 Bad Request` | Request is not in `Pending` status. |
| `404 Not Found` | Request not found. |

---

### Deny verification request

**`POST`** `/api/v1/admin/verification-requests/{id}/deny`

> 🛡️ **Admin**

Denies a pending identity verification request. Sends a notification email to the user with the denial reason.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `id` | `UUID` | Verification request ID. |

#### Request body (optional)

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `notes` | `string` | No | Reason for denial (included in notification email to the user). |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `id` | `string (UUID)` | Request ID. |
| `denied` | `boolean` | Always `true`. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Denied. |
| `400 Bad Request` | Request is not in `Pending` status. |
| `404 Not Found` | Request not found. |

---

### Process verification queue

**`POST`** `/api/v1/admin/verification-requests/process`

> 🛡️ **Admin**

Triggers the automated verification worker (placeholder). Currently returns a count of pending requests. Future: will run automated document verification logic.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `pendingCount` | `integer` | Number of pending requests. |
| `message` | `string` | Status message. |

---

## Files

### List files

**`GET`** `/api/v1/admin/files`

> 🛡️ **Admin**

Returns a paginated list of all stored files on the platform.

#### Query parameters

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `search` | `string` | No | — | Filename or content type search. |
| `page` | `integer` | No | `1` | Page number. |

Page size is fixed at 50.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `total` | `integer` | Total matching files. |
| `page` | `integer` | Current page. |
| `pageSize` | `integer` | Items per page (50). |
| `items` | `array` | File records. |

Each item: `id`, `originalFileName`, `fileName`, `contentType`, `lengthBytes`, `storageProvider`, `storagePath`, `uploadedByUserId`, `createdUtc`, `uploaderEmail`.

---

## Stripe Events

### List Stripe events

**`GET`** `/api/v1/admin/stripe-events`

> 🛡️ **Admin**

Returns a paginated list of Stripe payment events recorded by the webhook handler.

#### Query parameters

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `status` | `StripeEventStatus` | No | — | Filter by status: `Pending`, `Reviewed`, `Completed`, or `Failed`. |
| `page` | `integer` | No | `1` | Page number. |

Page size is fixed at 50.

#### Response `200 OK`

Each item:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Internal event record ID. |
| `stripeEventId` | `string` | No | Stripe event identifier. |
| `eventType` | `string` | No | Stripe event type string. |
| `amountTotal` | `number` | Yes | Charged amount. |
| `currency` | `string` | Yes | Currency code. |
| `customerEmail` | `string` | Yes | Customer email from Stripe. |
| `status` | `StripeEventStatus` | No | Processing status. |
| `internalNotes` | `string` | Yes | Admin notes. |
| `completedByUserId` | `string (UUID)` | Yes | Admin who completed processing. |
| `completedUtc` | `string (ISO 8601)` | Yes | When processed. |
| `createdUtc` | `string (ISO 8601)` | No | When the event arrived. |
| `organisationId` | `string (UUID)` | Yes | Linked organisation. |
| `organisationName` | `string` | Yes | Organisation name. |
| `purchaseType` | `string` | Yes | Purchase category. |
| `purchaseMetaJson` | `string` | Yes | JSON metadata from purchase. |

---

### Approve verification payment

**`POST`** `/api/v1/admin/stripe-events/{eventId}/approve-verification`

> 🛡️⬆️ **SuperAdmin/Admin**

Manually processes a Stripe verification purchase event that could not be automatically applied. Sets the organisation's verification tier based on the event metadata and marks the event `Completed`.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `eventId` | `UUID` | Stripe event record ID. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `organisationId` | `string (UUID)` | Organisation ID. |
| `tier` | `string` | Applied verification tier. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Tier applied. |
| `400 Bad Request` | Event has no associated organisation. |
| `404 Not Found` | Event or organisation not found. |

---

### Reject verification payment

**`POST`** `/api/v1/admin/stripe-events/{eventId}/reject-verification`

> 🛡️⬆️ **SuperAdmin/Admin**

Marks a Stripe verification payment as `Reviewed` (rejected) without applying a tier upgrade.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `eventId` | `UUID` | Stripe event record ID. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `reason` | `string` | No | Reason for rejection (stored as internal notes). |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `eventId` | `string (UUID)` | Event ID. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Event marked as rejected. |
| `404 Not Found` | Event not found. |

---

### Update Stripe event

**`PUT`** `/api/v1/admin/stripe-events/{eventId}`

> 🛡️ **Admin**

Manually updates the status and internal notes of a Stripe payment event record.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `eventId` | `UUID` | Stripe event record ID. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `status` | `StripeEventStatus` | Yes | New status. |
| `internalNotes` | `string` | No | Internal notes. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `eventId` | `string (UUID)` | Event ID. |
| `status` | `string` | Applied status. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Updated. |
| `404 Not Found` | Event not found. |

---

## System Settings

### List settings

**`GET`** `/api/v1/admin/settings`

> 🛡️ **Admin**

Returns all system settings key–value pairs. Secret keys (`GoogleIndexingServiceAccount`, `EmailMailerSendApiKey`) are masked to `••••••••` for non-SuperAdmin callers.

#### Response `200 OK`

Array of setting objects:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `key` | `string` | No | Setting key. |
| `value` | `string` | Yes | Setting value (masked for secrets if caller is not SuperAdmin). |
| `description` | `string` | Yes | Human-readable description. |
| `updatedUtc` | `string (ISO 8601)` | Yes | When last updated. |
| `updatedByUserId` | `string (UUID)` | Yes | Who last updated it. |

---

### Update setting

**`PUT`** `/api/v1/admin/settings/{key}`

> 🛡️ **Admin** (SuperAdmin required for secret keys)

Updates a system setting value. Writing to secret keys (`GoogleIndexingServiceAccount`, `EmailMailerSendApiKey`) requires SuperAdmin.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `key` | `string` | Setting key. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `value` | `string` | Yes | New value for the setting. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `key` | `string` | Setting key. |
| `updated` | `boolean` | Always `true`. |

---

### Delete setting

**`DELETE`** `/api/v1/admin/settings/{key}`

> 🛡️⬆️⬆️ **SuperAdmin only**

Removes a system setting entirely. The platform will fall back to its default value for this setting.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `key` | `string` | Setting key. |

#### Response `204 No Content`

#### Status codes

| Status | Meaning |
| --- | --- |
| `204 No Content` | Deleted. |
| `404 Not Found` | Setting not found. |

---

### Get import API key status

**`GET`** `/api/v1/admin/settings/import-api-key`

> 🛡️⬆️ **SuperAdmin/Admin**

Returns the current import API key status. The key value is masked — only the last 8 characters are visible. Use this to confirm a key is configured without exposing the full value.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `isConfigured` | `boolean` | `true` when a key is set (either in DB or env var). |
| `maskedKey` | `string` | The key with all but the last 8 characters replaced by `•`. Empty string if not configured. |
| `storedInDb` | `boolean` | `true` when the key comes from the `Import.ApiKey` system setting. |
| `storedInEnv` | `boolean` | `true` when the key falls back to the `IMPORT_API_KEY` environment variable. |

---

### Rotate import API key

**`POST`** `/api/v1/admin/settings/import-api-key/rotate`

> 🛡️⬆️ **SuperAdmin/Admin**

Generates a new cryptographically-random 64-character API key and saves it to the `Import.ApiKey` system setting. The previous key is immediately invalidated.

The new key uses URL-safe Base64 encoding (no `+`, `/`, or `=` characters).

After rotating, update the `X-Import-Api-Key` value in any external cron jobs or feed ingesters before the next run.

#### Response `200 OK`

```json
{
  "newKey": "aB3dE5fG7hI9jK1lM3nO5pQ7rS9tU1vW3xY5zA7bC9dE1fG3hI5jK7lM9nO1pQ3rS5t"
}
```

| Field | Type | Description |
| --- | --- | --- |
| `newKey` | `string` | The newly generated API key — store this immediately, it is not shown again. |

---

## System Logs

### List logs

**`GET`** `/api/v1/admin/logs`

> 🛡️ **Admin**

Returns paginated system log entries.

#### Query parameters

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `level` | `SystemLogLevel` | No | — | Filter by log level: `Debug`, `Info`, `Warning`, `Error`, or `Critical`. |
| `category` | `string` | No | — | Filter by log category. |
| `search` | `string` | No | — | Search message or details. |
| `page` | `integer` | No | `1` | Page number. |
| `pageSize` | `integer` | No | `50` | Items per page (max 200). |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `total` | `integer` | Total matching entries. |
| `page` | `integer` | Current page. |
| `pageSize` | `integer` | Items per page. |
| `items` | `array` | Log entries. |

Each item:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Log entry ID. |
| `timestampUtc` | `string (ISO 8601)` | No | When the event was logged. |
| `level` | `SystemLogLevel` | No | Log level. |
| `category` | `string` | Yes | Log category/source. |
| `message` | `string` | No | Log message. |
| `details` | `string` | Yes | Extended details. |
| `exceptionType` | `string` | Yes | Exception class name if an error. |
| `exceptionMessage` | `string` | Yes | Exception message. |
| `userId` | `string (UUID)` | Yes | User associated with the log entry. |
| `requestPath` | `string` | Yes | HTTP request path. |

---

### Purge logs

**`DELETE`** `/api/v1/admin/logs`

> 🛡️⬆️⬆️ **SuperAdmin only**

Deletes log entries older than a specified number of days.

#### Query parameters

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `olderThanDays` | `integer` | No | `30` | Delete entries older than this many days. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `deleted` | `integer` | Number of entries deleted. |
| `cutoffDate` | `string (ISO 8601)` | Cutoff timestamp used. |

---

## Locations

### List locations

**`GET`** `/api/v1/admin/locations`

> 🛡️ **Admin**

Returns a paginated list of all configured locations used in job postings and searches.

#### Query parameters

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `search` | `string` | No | — | Display name search. |
| `page` | `integer` | No | `1` | Page number. |

Page size is fixed at 100.

#### Response `200 OK`

Array of location objects:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Location ID. |
| `displayName` | `string` | No | Display name shown in the UI. |
| `city` | `string` | Yes | City name. |
| `state` | `string` | Yes | State or region. |
| `country` | `string` | Yes | Country name. |
| `countryCode` | `string` | Yes | ISO 3166-1 alpha-2 country code. |
| `latitude` | `number` | No | Latitude coordinate. |
| `longitude` | `number` | No | Longitude coordinate. |
| `isActive` | `boolean` | No | Whether the location is available for use. |
| `sortOrder` | `integer` | No | Display order in dropdowns. |
| `createdUtc` | `string (ISO 8601)` | No | Creation date. |

---

### Add location

**`POST`** `/api/v1/admin/locations`

> 🛡️⬆️ **SuperAdmin/Admin**

Adds a single location record.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `displayName` | `string` | Yes | Display name. |
| `city` | `string` | No | City name. |
| `state` | `string` | No | State or region. |
| `country` | `string` | No | Country name. |
| `countryCode` | `string` | No | ISO country code (converted to uppercase). |
| `latitude` | `number` | Yes | Latitude. |
| `longitude` | `number` | Yes | Longitude. |
| `isActive` | `boolean` | No | Defaults to `true`. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `locationId` | `string (UUID)` | Created location ID. |
| `displayName` | `string` | Display name. |

---

### Bulk add locations

**`POST`** `/api/v1/admin/locations/bulk`

> 🛡️⬆️ **SuperAdmin/Admin**

Adds multiple locations in a single request. Uses the same field structure as [Add location](#add-location) but accepts an array.

#### Request body

Array of location objects (same fields as single add).

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `added` | `integer` | Number of locations created. |

---

### Update location

**`PUT`** `/api/v1/admin/locations/{locationId}`

> 🛡️⬆️ **SuperAdmin/Admin**

Updates an existing location record.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `locationId` | `UUID` | Location ID. |

#### Request body

Same fields as [Add location](#add-location).

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `locationId` | `string (UUID)` | Location ID. |
| `updated` | `boolean` | Always `true`. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Updated. |
| `404 Not Found` | Location not found. |

---

### Delete location

**`DELETE`** `/api/v1/admin/locations/{locationId}`

> 🛡️⬆️ **SuperAdmin/Admin**

Deletes a location record.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `locationId` | `UUID` | Location ID. |

#### Response `204 No Content`

#### Status codes

| Status | Meaning |
| --- | --- |
| `204 No Content` | Deleted. |
| `404 Not Found` | Location not found. |

---

## Syndication

### List syndication records

**`GET`** `/api/v1/admin/syndication-records`

> 🛡️ **Admin**

Returns job syndication records tracking external job board submissions.

#### Query parameters

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `page` | `integer` | No | `1` | Page number. |

Page size is fixed at 100.

#### Response `200 OK`

Array of syndication records:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Record ID. |
| `jobId` | `string (UUID)` | No | Job ID. |
| `jobTitle` | `string` | No | Job title. |
| `provider` | `string` | No | External provider name (e.g. `GoogleIndexing`). |
| `status` | `string` | No | Submission status. |
| `externalRef` | `string` | Yes | Reference ID from the external provider. |
| `submittedUtc` | `string (ISO 8601)` | No | When submitted. |
| `lastAttemptUtc` | `string (ISO 8601)` | Yes | Most recent submission attempt. |
| `lastErrorMessage` | `string` | Yes | Error from last failed attempt. |

---

## Compose Email

### Send email to user

**`POST`** `/api/v1/admin/compose-email`

> 🛡️ **Admin**

Sends a custom HTML email to a specific user. The email is wrapped in the platform's standard branded template. Requires outbound email (`Email__FromEmailSupport`) to be configured.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `userId` | `string (UUID)` | Yes | ID of the recipient user. |
| `subject` | `string` | Yes | Email subject line. |
| `htmlBody` | `string` | Yes | HTML email body. Wrapped in branded template before sending. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `sent` | `boolean` | Always `true`. |
| `toEmail` | `string` | Recipient email address. |
| `toName` | `string` | Recipient display name. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Email sent. |
| `404 Not Found` | User not found. |
| `503 Service Unavailable` | Email is not configured (`Email__FromEmailSupport` not set). |

---

## Account Maintenance

### Delete job seeker account

**`DELETE`** `/api/v1/admin/job-seekers/{userId}/account`

> 🛡️⬆️ **SuperAdmin/Admin**

Permanently deletes a job seeker account and all associated data (profile, files, applications). The email address is also removed from the identity record so the user can re-register. This action is irreversible.

Deletion order: non-hired applications → remaining applications → job seeker profile → stored files → Identity user record.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `userId` | `UUID` | Job seeker user ID. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `message` | `string` | Confirmation message. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Account deleted. |
| `400 Bad Request` | User is not a job seeker account type. |
| `404 Not Found` | User not found. |

---

### Purge inactive school leavers

**`POST`** `/api/v1/admin/maintenance/purge-inactive-school-leavers`

> 🛡️⬆️ **SuperAdmin/Admin**

Deletes all school leaver job seeker accounts (age group `SchoolLeaver`) that have not logged in for 30 or more days. This is a privacy compliance operation per the platform's age policy.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `purged` | `integer` | Number of accounts deleted. |
| `cutoffUtc` | `string (ISO 8601)` | Inactivity cutoff timestamp used. |

---

### Purge inactive adults

**`POST`** `/api/v1/admin/maintenance/purge-inactive-adults`

> 🛡️⬆️ **SuperAdmin/Admin**

Deletes all adult job seeker accounts (age group `Adult`) that have not logged in for 90 or more days **and** have never been hired. Accounts with at least one `Hired` application are retained regardless of inactivity.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `purged` | `integer` | Number of accounts deleted. |
| `cutoffUtc` | `string (ISO 8601)` | Inactivity cutoff timestamp used. |
