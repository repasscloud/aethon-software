---
title: Aethon API Documentation
description: Developer reference for the Aethon Applicant Tracking System API
weight: 0
---

# Aethon API Documentation

This directory contains the developer reference for the Aethon ATS REST API.

The API is a .NET 10 Minimal API backed by PostgreSQL. All responses are JSON.
Base URL: `https://<host>/api/v1`

---

## Contents

### Getting started

| Document | Description |
| --- | --- |
| [API Overview](api/00-overview.md) | Base URL, authentication, pagination, errors, conventions |
| [Enums](models/enums.md) | All enumeration values used in requests and responses |
| [Data Schemas](models/schemas.md) | Shared DTO shapes referenced across multiple endpoints |
| [Style Guide](style-guide.md) | Documentation conventions (for contributors) |

### API reference — by audience

#### Public / unauthenticated

| Document | Routes |
| --- | --- |
| [Public](api/02-public.md) | `/api/v1/public/*` — job search, org profiles, apply |

#### Job seekers

| Document | Routes |
| --- | --- |
| [Authentication](api/01-auth.md) | `/api/v1/auth/*` — register, login, MFA, password reset |
| [Candidates](api/03-candidates.md) | `/api/v1/me/*` — profile, skills, work history, account deletion |
| [Applications](api/05-applications.md) | `/api/v1/applications/*` — apply, track, withdraw |
| [Files](api/08-files.md) | `/api/v1/files/*` — upload resume, download files |
| [Identity](api/09-identity.md) | `/api/v1/identity/*` — identity verification requests |

#### Employers (companies)

| Document | Routes |
| --- | --- |
| [Authentication](api/01-auth.md) | `/api/v1/auth/*` |
| [Jobs](api/04-jobs.md) | `/api/v1/jobs/*` — create, publish, manage jobs |
| [Applications](api/05-applications.md) | `/api/v1/applications/*` — review, shortlist, hire |
| [Organisations](api/06-organisations.md) | `/api/v1/organisations/*` — profile, team, domains |
| [Billing](api/07-billing.md) | `/api/v1/billing/*` — credits, checkout, Stripe portal |
| [Company Jobs](api/11-company-jobs.md) | `/api/v1/company/jobs/*` — approve recruiter job submissions |
| [Company Recruiters](api/13-company-recruiters.md) | `/api/v1/company/recruiters/*` — manage recruiter partnerships |

#### Recruitment agencies

| Document | Routes |
| --- | --- |
| [Authentication](api/01-auth.md) | `/api/v1/auth/*` |
| [Recruiter Jobs](api/10-recruiter-jobs.md) | `/api/v1/recruiter/jobs/*` — draft and submit jobs for company approval |
| [Recruiter Companies](api/12-recruiter-companies.md) | `/api/v1/recruiter/companies/*` — manage company partnerships |
| [Organisations](api/06-organisations.md) | `/api/v1/organisations/*` |
| [Integrations](api/14-integrations.md) | `/api/v1/integrations/*` — webhooks |

#### Platform administration

| Document | Routes |
| --- | --- |
| [Admin](api/15-admin.md) | `/api/v1/admin/*` — users, orgs, jobs, settings, purge |

---

## Authentication summary

| Scenario | Token |
| --- | --- |
| All authenticated endpoints | `Authorization: Bearer <jwt>` |
| Token lifetime | Configured via `Jwt:ExpiryMinutes` setting |
| Token source | `POST /api/v1/auth/login` or `POST /api/v1/auth/verify-2fa` |

Roles embedded in the JWT: `SuperAdmin`, `Admin`, `Support`.
Account type is available as `appType` in the login response (`jobseeker`, `employer`, `recruiter`).

---

## Age policy

Aethon enforces age classification for job seekers in compliance with AU APPs, GDPR, and CCPA/CPRA.

| Age group | Value | Notes |
| --- | --- | --- |
| Not confirmed | `NotSpecified` | Cannot apply for jobs until confirmed |
| School leaver | `SchoolLeaver` | 16–18. Birth month + year stored (no full DOB). |
| Adult | `Adult` | 18+. No date stored. |

Job postings carry two flags:

| Flag | Meaning |
| --- | --- |
| `isSuitableForSchoolLeavers` | School leavers may apply alongside adults |
| `isSchoolLeaverTargeted` | Only school leavers can see and apply for this job |

---

## Version

Current API version: **v1** (all routes prefixed `/api/v1/`).
