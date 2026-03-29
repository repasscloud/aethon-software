---
title: Schema Reference
description: Shared DTO shapes referenced across multiple API endpoints
weight: 20
---

# Schema Reference

This page documents shared data shapes referenced from multiple API endpoints.

---

## EmployerJobApplicationDetailDto

Full application detail returned to employers and recruiters via [`GET /applications/{applicationId}`](../api/05-applications.md#get-application).

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Application ID. |
| `jobId` | `string (UUID)` | No | Job ID. |
| `jobTitle` | `string` | No | Job title at time of application. |
| `applicantUserId` | `string (UUID)` | No | Applicant user ID. |
| `applicantDisplayName` | `string` | No | Applicant display name. |
| `applicantEmail` | `string` | No | Applicant email. |
| `status` | `ApplicationStatus` | No | Current status. |
| `coverLetter` | `string` | Yes | Cover letter text. |
| `resumeFileId` | `string (UUID)` | Yes | Stored file ID of the attached resume. |
| `resumeDownloadUrl` | `string` | Yes | Direct download URL for the resume. |
| `source` | `string` | Yes | Application source (e.g. `JobBoard`, `DirectEmail`). |
| `notes` | `string` | Yes | Internal notes. |
| `submittedUtc` | `string (ISO 8601)` | No | When the application was submitted. |
| `lastStatusChangedUtc` | `string (ISO 8601)` | Yes | When the status last changed. |

---

## PublicJobDetailDto

Full public job detail returned by [`GET /public/jobs/{jobId}`](../api/02-public.md#get-job-detail).

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Job ID. |
| `title` | `string` | No | Job title. |
| `summary` | `string` | Yes | Short job summary. |
| `department` | `string` | Yes | Department or team. |
| `locationText` | `string` | Yes | Location display string. |
| `locationCity` | `string` | Yes | City. |
| `locationState` | `string` | Yes | State or region. |
| `locationCountry` | `string` | Yes | Country. |
| `locationCountryCode` | `string` | Yes | ISO 3166-1 alpha-2 country code. |
| `locationLatitude` | `number` | Yes | Latitude. |
| `locationLongitude` | `number` | Yes | Longitude. |
| `workplaceType` | `WorkplaceType` | No | `OnSite`, `Hybrid`, or `Remote`. |
| `employmentType` | `EmploymentType` | No | Employment type. |
| `description` | `string` | No | Full job description (HTML). |
| `requirements` | `string` | Yes | Requirements section (HTML). |
| `benefits` | `string` | Yes | Benefits section (HTML). |
| `salaryFrom` | `number` | Yes | Minimum salary. |
| `salaryTo` | `number` | Yes | Maximum salary. |
| `salaryCurrency` | `CurrencyCode` | Yes | Salary currency. |
| `hasCommission` | `boolean` | No | Whether the role includes commission. |
| `oteFrom` | `number` | Yes | OTE minimum (on-target earnings). |
| `oteTo` | `number` | Yes | OTE maximum. |
| `publishedUtc` | `string (ISO 8601)` | Yes | When the job was published. |
| `postingExpiresUtc` | `string (ISO 8601)` | Yes | When the posting expires. |
| `category` | `JobCategory` | Yes | Job category. |
| `regions` | `JobRegion[]` | No | Target regions. |
| `countries` | `string[]` | No | Target countries. |
| `benefitsTags` | `string[]` | No | Tagged benefits (e.g. `RemoteWork`, `FlexibleHours`). |
| `applicationSpecialRequirements` | `string` | Yes | Special requirements for applicants. |
| `externalApplicationUrl` | `string` | Yes | URL to apply externally. |
| `applicationEmail` | `string` | Yes | Email to send applications to. |
| `isImmediateStart` | `boolean` | No | Whether an immediate start is available. |
| `videoYouTubeId` | `string` | Yes | YouTube video ID for the job. |
| `videoVimeoId` | `string` | Yes | Vimeo video ID for the job. |
| `screeningQuestionsJson` | `string` | Yes | JSON-serialised screening questions config. |
| `organisation` | `PublicOrganisationProfileDto` | No | Employer organisation profile. |

---

## PublicOrganisationProfileDto

Organisation profile embedded in public job listings and job detail pages.

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Organisation ID. |
| `name` | `string` | No | Organisation name. |
| `slug` | `string` | Yes | Public profile slug. |
| `logoUrl` | `string` | Yes | Logo image URL. |
| `website` | `string` | Yes | Website URL. |
| `industry` | `string` | Yes | Industry sector. |
| `companySize` | `CompanySize` | Yes | Employee count range. |
| `verificationTier` | `VerificationTier` | No | Verification status. |
| `isEqualOpportunityEmployer` | `boolean` | No | Equal opportunity employer flag. |
| `isAccessibleWorkplace` | `boolean` | No | Accessible workplace flag. |

---

## PagedResult&lt;T&gt;

Standard paginated response wrapper used by list endpoints.

| Field | Type | Description |
| --- | --- | --- |
| `items` | `T[]` | Array of result items. |
| `total` | `integer` | Total number of matching items across all pages. |
| `page` | `integer` | Current page number (1-based). |
| `pageSize` | `integer` | Items per page. |

---

## JobListItemDto

Job summary used in employer/recruiter job list responses.

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Job ID. |
| `title` | `string` | No | Job title. |
| `department` | `string` | Yes | Department. |
| `locationText` | `string` | Yes | Location display string. |
| `workplaceType` | `WorkplaceType` | No | Workplace arrangement. |
| `employmentType` | `EmploymentType` | No | Employment type. |
| `status` | `JobStatus` | No | Job lifecycle status. |
| `salaryFrom` | `number` | Yes | Minimum salary. |
| `salaryTo` | `number` | Yes | Maximum salary. |
| `salaryCurrency` | `CurrencyCode` | Yes | Salary currency. |
| `createdUtc` | `string (ISO 8601)` | No | Creation date. |
| `publishedUtc` | `string (ISO 8601)` | Yes | Published date. |

---

## PublicJobListItemDto

Job summary used in public job board list responses.

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Job ID. |
| `title` | `string` | No | Job title. |
| `organisationName` | `string` | No | Employer name. |
| `organisationSlug` | `string` | Yes | Employer profile slug. |
| `organisationLogoUrl` | `string` | Yes | Employer logo URL. |
| `locationText` | `string` | Yes | Location display string. |
| `workplaceType` | `WorkplaceType` | No | Workplace arrangement. |
| `employmentType` | `EmploymentType` | No | Employment type. |
| `category` | `JobCategory` | Yes | Job category. |
| `salaryFrom` | `number` | Yes | Minimum salary. |
| `salaryTo` | `number` | Yes | Maximum salary. |
| `salaryCurrency` | `CurrencyCode` | Yes | Salary currency. |
| `publishedUtc` | `string (ISO 8601)` | Yes | When published. |
| `postingExpiresUtc` | `string (ISO 8601)` | Yes | When posting expires. |
| `isSuitableForSchoolLeavers` | `boolean` | No | Whether the role accepts school leaver applications. |
| `isSchoolLeaverTargeted` | `boolean` | No | Whether the role is specifically for school leavers (hidden from non-school-leavers). |

---

## WebhookSubscriptionDto

Webhook subscription record returned by integration endpoints.

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Subscription ID. |
| `organisationId` | `string (UUID)` | No | Owning organisation ID. |
| `name` | `string` | No | Subscription display name. |
| `endpointUrl` | `string` | No | URL receiving webhook POST requests. |
| `secret` | `string` | No | HMAC signing secret. |
| `isActive` | `boolean` | No | Whether the subscription is active. |
| `events` | `string[]` | No | Subscribed event names. |

---

## RecruiterCompanyRelationshipDto

Partnership relationship between a recruiter agency and a company.

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Partnership ID. |
| `recruiterOrganisationId` | `string (UUID)` | No | Recruiter org ID. |
| `companyOrganisationId` | `string (UUID)` | No | Company org ID. |
| `recruiterOrganisationName` | `string` | No | Recruiter org name. |
| `companyOrganisationName` | `string` | No | Company org name. |
| `status` | `OrganisationRecruitmentPartnershipStatus` | No | Partnership status. |
| `scope` | `OrganisationRecruitmentPartnershipScope` | No | Permission scope flags. |
| `recruiterCanCreateUnclaimedCompanyJobs` | `boolean` | No | Permission flag. |
| `recruiterCanPublishJobs` | `boolean` | No | Permission flag. |
| `recruiterCanManageCandidates` | `boolean` | No | Permission flag. |
| `requestedByUserId` | `string (UUID)` | Yes | User who created the request. |
| `approvedByUserId` | `string (UUID)` | Yes | User who approved it. |
| `createdUtc` | `string (ISO 8601)` | No | When created. |
| `approvedUtc` | `string (ISO 8601)` | Yes | When approved. |
| `notes` | `string` | Yes | Notes from approval. |

---

## ErrorResponse

Standard error response shape returned on `4xx` and `5xx` responses.

```json
{
  "code": "error.code_key",
  "message": "Human-readable description.",
  "details": {}
}
```

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `code` | `string` | No | Machine-readable error code (namespaced, snake_case). |
| `message` | `string` | No | Human-readable description. |
| `details` | `object` | Yes | Additional structured context (field errors, etc.). |

Validation errors (`400 Bad Request`) use RFC 7807 `ValidationProblemDetails` format:

```json
{
  "title": "One or more validation errors occurred.",
  "errors": {
    "fieldName": ["Error message 1", "Error message 2"]
  }
}
```
