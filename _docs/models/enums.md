---
title: Enum Reference
description: All enum types used in Aethon API requests and responses
weight: 10
---

# Enum Reference

All enums are serialised as their string name in JSON responses (e.g. `"Active"`, not `2`). Where used as query parameters, values are case-insensitive.

Flags enums (`OrganisationRecruitmentPartnershipScope`) serialise as a comma-separated string of set values.

---

## ApplicantAgeGroup

Age classification for a job seeker. Set on the profile; required before applying to any job.

| Value | Description |
| --- | --- |
| `NotSpecified` | Age group not yet confirmed. |
| `SchoolLeaver` | Aged 16–18. Birth month and year stored; no full DOB captured. |
| `Adult` | Confirmed 18 or older. No date of birth captured. |

---

## ApplicationStatus

Workflow status of a job application.

| Value | Description |
| --- | --- |
| `Draft` | Not yet submitted. |
| `Submitted` | Submitted by the candidate. |
| `UnderReview` | Being reviewed by the employer. |
| `Shortlisted` | Candidate has been shortlisted. |
| `Interview` | Interview scheduled or completed. |
| `Offer` | Offer extended. |
| `Hired` | Candidate was hired. |
| `Rejected` | Application rejected. |
| `Withdrawn` | Withdrawn by the candidate. |

Valid transitions: `Submitted → UnderReview → Shortlisted → Interview → Offer → Hired` or `→ Rejected`. `Submitted → Rejected` and `→ Withdrawn` are also valid.

---

## ClaimRequestStatus

Status of an organisation claim request.

| Value | Description |
| --- | --- |
| `Pending` | Awaiting review. |
| `Approved` | Claim approved. |
| `Rejected` | Claim rejected. |
| `Cancelled` | Cancelled by the requester. |
| `Expired` | Request expired before review. |

---

## CompanyRole

Role of a member within a company organisation.

| Value | Description |
| --- | --- |
| `Owner` | Organisation owner. |
| `Admin` | Administrator with full management access. |
| `Recruiter` | Can create and manage jobs. |
| `HiringManager` | Can review applications and conduct interviews. |
| `Interviewer` | Can view and conduct interviews. |
| `Viewer` | Read-only access. |

---

## CompanySize

Employee count range for an organisation.

| Value | Description |
| --- | --- |
| `UpTo10` | 1–10 employees. |
| `UpTo50` | 11–50 employees. |
| `UpTo200` | 51–200 employees. |
| `UpTo500` | 201–500 employees. |
| `UpTo1000` | 501–1,000 employees. |
| `UpTo5000` | 1,001–5,000 employees. |
| `UpTo10000` | 5,001–10,000 employees. |
| `MoreThan10000` | 10,000+ employees. |

---

## CreditSource

How job posting credits were acquired.

| Value | Description |
| --- | --- |
| `LaunchPromotion` | Granted as part of a launch promotion. |
| `StripePurchase` | Purchased via Stripe. |
| `AdminGrant` | Manually granted by an administrator. |
| `ConvertedFromStandard` | Converted from Standard to Premium upon verification. |

---

## CreditType

Type of job posting credit.

| Value | Description |
| --- | --- |
| `JobPostingStandard` | Standard job posting credit. |
| `JobPostingPremium` | Premium job posting credit. |
| `StickyTop24h` | Sticky top placement for 24 hours. |
| `StickyTop7d` | Sticky top placement for 7 days. |
| `StickyTop30d` | Sticky top placement for 30 days. |

---

## CurrencyCode

Supported salary currency codes.

| Value | Description |
| --- | --- |
| `AUD` | Australian Dollar. |
| `USD` | US Dollar. |
| `EUR` | Euro. |
| `GBP` | British Pound. |
| `NZD` | New Zealand Dollar. |

---

## DomainStatus

Status of an organisation's verified domain.

| Value | Description |
| --- | --- |
| `Pending` | Awaiting DNS verification. |
| `Verified` | Domain verified. |
| `Rejected` | Verification failed. |
| `Disabled` | Domain disabled. |

---

## DomainTrustLevel

Trust level assigned to a verified domain.

| Value | Description |
| --- | --- |
| `Low` | Low trust — basic verification. |
| `Medium` | Medium trust. |
| `High` | High trust — full verification. |

---

## DomainVerificationMethod

Method used to verify an organisation domain.

| Value | Description |
| --- | --- |
| `None` | No verification method. |
| `DnsTxt` | DNS TXT record verification. |
| `Email` | Email confirmation. |
| `Manual` | Manually verified by an admin. |

---

## EmploymentType

Employment arrangement for a job.

| Value | Description |
| --- | --- |
| `FullTime` | Full-time permanent. |
| `PartTime` | Part-time. |
| `Contract` | Fixed-term contract. |
| `Temporary` | Temporary placement. |
| `Casual` | Casual/on-call. |
| `Internship` | Internship or placement. |

---

## InterviewStatus

Status of a scheduled interview.

| Value | Description |
| --- | --- |
| `Scheduled` | Interview is scheduled. |
| `Completed` | Interview was completed. |
| `Cancelled` | Interview was cancelled. |
| `NoShow` | Candidate did not attend. |
| `Rescheduled` | Interview has been rescheduled. |

---

## InterviewType

Format of an interview.

| Value | Description |
| --- | --- |
| `PhoneScreen` | Initial phone screening. |
| `VideoInterview` | Video call interview. |
| `InPerson` | In-person meeting. |
| `TechnicalAssessment` | Technical test or coding challenge. |
| `PanelInterview` | Panel with multiple interviewers. |
| `FinalInterview` | Final round interview. |
| `Other` | Other format. |

---

## InvitationStatus

Status of an invitation.

| Value | Description |
| --- | --- |
| `Pending` | Not yet accepted or declined. |
| `Accepted` | Accepted by the recipient. |
| `Expired` | Invitation link expired. |
| `Cancelled` | Cancelled by the sender. |

---

## InvitationType

Type of invitation sent.

| Value | Description |
| --- | --- |
| `JoinOrganisation` | Invitation to join an organisation. |
| `ClaimOrganisation` | Invitation to claim an unclaimed organisation. |
| `DomainVerificationEmail` | Domain ownership verification email. |

---

## JobCategory

Industry category for a job posting.

`Accounting`, `AdminSecretarial`, `AdvertisingPR`, `Aerospace`, `AgricultureFishingForestry`, `Arts`, `Automobile`, `Banking`, `BuildingConstruction`, `Catering`, `Charity`, `CustomerService`, `Design`, `Education`, `Engineering`, `ExecutiveManagement`, `FinanceInsurance`, `FoodBeverage`, `Government`, `GraduateRoles`, `Healthcare`, `Hospitality`, `HumanResources`, `ITSoftware`, `LegalServices`, `Logistics`, `Manufacturing`, `Marketing`, `MediaJournalism`, `MiningResources`, `PartTimeTemp`, `Pharmaceuticals`, `PropertyRealEstate`, `PublicRelations`, `Recruitment`, `Research`, `Retail`, `Sales`, `Science`, `SecurityIntelligence`, `SocialWork`, `SportRecreation`, `TelecommunicationsISP`, `Tourism`, `TransportDistribution`, `UtilitiesEnergy`, `Veterinary`, `Other`

---

## JobCreatedByType

Who created the job record.

| Value | Description |
| --- | --- |
| `CompanyUser` | Created by a company member. |
| `RecruiterUser` | Created by a recruiter agency member. |
| `PlatformAdmin` | Created by a platform administrator. |

---

## JobPostingTier

Posting tier that determines feature set and credit cost.

| Value | Description |
| --- | --- |
| `Standard` | Standard job posting. |
| `Premium` | Premium job posting with enhanced visibility. |

---

## JobRegion

Geographic region for job visibility targeting.

| Value | Description |
| --- | --- |
| `Africa` | Africa region. |
| `Asia` | Asia region. |
| `Europe` | Europe region. |
| `LatinAmerica` | Latin America region. |
| `MiddleEast` | Middle East region. |
| `NorthAmerica` | North America region. |
| `Oceania` | Oceania / Pacific region. |
| `Worldwide` | No region restriction. |

---

## JobStatus

Lifecycle status of a job posting.

| Value | Description |
| --- | --- |
| `Draft` | Under construction, not submitted. |
| `PendingCompanyApproval` | Submitted by recruiter, awaiting company approval. |
| `Approved` | Approved by the company, ready to publish. |
| `Published` | Live on the job board. |
| `OnHold` | Temporarily removed from the job board. |
| `Closed` | Position filled or no longer accepting applications. |
| `Cancelled` | Cancelled before publishing. |

---

## JobVisibility

Controls who can see the job listing.

| Value | Description |
| --- | --- |
| `Private` | Not visible on the public job board. |
| `Public` | Visible on the public job board. |

---

## LanguageAbilityType

Type of language ability.

| Value | Description |
| --- | --- |
| `Spoken` | Spoken language only. |
| `Written` | Written language only. |
| `Both` | Both spoken and written. |

---

## LanguageProficiencyLevel

Level of language proficiency.

| Value | Description |
| --- | --- |
| `Basic` | Basic — can communicate in simple situations. |
| `Conversational` | Can hold everyday conversations. |
| `Professional` | Business-level proficiency. |
| `NativeOrBilingual` | Native speaker or bilingual. |

---

## MembershipStatus

Status of a user's membership within an organisation.

| Value | Description |
| --- | --- |
| `Pending` | Invitation sent, not yet accepted. |
| `Active` | Active member. |
| `Suspended` | Access temporarily suspended. |
| `Revoked` | Access permanently revoked. |

---

## OrganisationClaimStatus

Whether an organisation has been claimed by a user.

| Value | Description |
| --- | --- |
| `NotApplicable` | Claim status is not applicable. |
| `Unclaimed` | Organisation exists but has not been claimed. |
| `Claimed` | Organisation has been claimed by a registered user. |

---

## OrganisationRecruitmentPartnershipScope

Flags enum defining the operations a recruiter is allowed to perform for a company partner. Values can be combined.

| Flag | Value | Description |
| --- | --- | --- |
| `None` | 0 | No permissions. |
| `CreateDraftJobs` | 1 | Can create draft jobs. |
| `SubmitJobsForApproval` | 2 | Can submit jobs for company approval. |
| `ManageApprovedJobs` | 4 | Can edit approved jobs. |
| `ViewCandidates` | 8 | Can view candidate profiles. |
| `SubmitCandidates` | 16 | Can submit candidates for jobs. |
| `CommunicateWithCandidates` | 32 | Can communicate with candidates. |
| `ScheduleInterviews` | 64 | Can schedule interviews. |
| `PublishJobs` | 128 | Can publish jobs directly. |

---

## OrganisationRecruitmentPartnershipStatus

Status of a recruiter–company partnership.

| Value | Description |
| --- | --- |
| `Pending` | Request submitted, awaiting company approval. |
| `Active` | Partnership is active. |
| `Suspended` | Partnership temporarily suspended by the company. |
| `Revoked` | Partnership permanently revoked. |
| `Rejected` | Partnership request rejected. |

---

## OrganisationStatus

Lifecycle status of an organisation record.

| Value | Description |
| --- | --- |
| `Draft` | Organisation record created but not yet provisioned. |
| `Provisioned` | Provisioned — awaiting activation. |
| `Claimable` | Available for a user to claim. |
| `Active` | Fully active. |
| `Suspended` | Suspended by platform administrators. |
| `Archived` | Archived — no longer active. |

---

## OrganisationType

Type of organisation.

| Value | Description |
| --- | --- |
| `Company` | Employer company. |
| `RecruiterAgency` | Recruiter agency. |

---

## ProfileVisibility

Visibility of a public profile page.

| Value | Description |
| --- | --- |
| `Private` | Profile is not publicly accessible. |
| `Unlisted` | Profile is accessible by direct link but not indexed. |
| `Public` | Profile is publicly accessible and indexed. |

---

## RecruiterRole

Role of a member within a recruiter agency organisation.

| Value | Description |
| --- | --- |
| `Owner` | Agency owner. |
| `Admin` | Administrator. |
| `Recruiter` | Standard recruiter. |
| `TeamLead` | Team lead. |
| `Viewer` | Read-only. |

---

## ResumeAnalysisStatus

Status of an AI resume analysis job.

| Value | Description |
| --- | --- |
| `Pending` | Queued for analysis. |
| `Processing` | Currently being analysed. |
| `Completed` | Analysis complete. |
| `Failed` | Analysis failed. |

---

## SkillLevel

Self-assessed skill proficiency level.

| Value | Description |
| --- | --- |
| `Beginner` | Beginner / learning. |
| `Intermediate` | Intermediate. |
| `Advanced` | Advanced. |
| `Expert` | Expert / specialist. |

---

## StripeEventStatus

Processing status of a Stripe payment event record.

| Value | Description |
| --- | --- |
| `Pending` | Event received, not yet processed. |
| `Reviewed` | Reviewed by an admin (manual rejection uses this). |
| `Completed` | Successfully processed. |
| `Failed` | Processing failed. |

---

## SystemLogLevel

Severity level for system log entries.

| Value | Description |
| --- | --- |
| `Debug` | Diagnostic detail. |
| `Info` | Informational event. |
| `Warning` | Potential issue. |
| `Error` | Error that should be investigated. |
| `Critical` | Critical failure requiring immediate attention. |

---

## UserAccountType

High-level account type set during registration.

| Value | Description |
| --- | --- |
| `Admin` | Platform administrator. |
| `Company` | Company employer account. |
| `RecruiterAgency` | Recruiter agency account. |
| `JobSeeker` | Job seeker account. |
| `Support` | Platform support staff account. |

---

## UserStatus

Account status.

| Value | Description |
| --- | --- |
| `PendingVerification` | Email not yet verified. |
| `Active` | Active account. |
| `Suspended` | Account suspended by admin. |
| `Disabled` | Account disabled. |

---

## VerificationRequestStatus

Status of a job seeker's identity verification request.

| Value | Description |
| --- | --- |
| `Pending` | Submitted, awaiting review. |
| `Approved` | Identity verified. |
| `Denied` | Identity could not be verified. |

---

## VerificationReviewerType

Who performed the identity verification review.

| Value | Description |
| --- | --- |
| `System` | Automated system review. |
| `OrgOwner` | Organisation owner. |
| `Admin` | Platform administrator. |

---

## VerificationTier

Organisation trust level granted after identity/business verification.

| Value | Description |
| --- | --- |
| `None` | Not verified. |
| `StandardEmployer` | Standard employer verification. |
| `EnhancedTrusted` | Enhanced trusted employer verification. |

---

## WorkplaceType

Workplace arrangement for a job.

| Value | Description |
| --- | --- |
| `OnSite` | On-site only. |
| `Hybrid` | Mix of on-site and remote. |
| `Remote` | Fully remote. |
