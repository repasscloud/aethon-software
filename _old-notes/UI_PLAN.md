# Aethon UI Plan
_Generated 2026-03-20 — full audit of existing pages, endpoints, and gaps_

---

## Immediate Bugs (Before Any UI Work)

These are already broken in the running system and block everything else.

### BUG-01 · Login claims not populated → "No app type resolved"
The web login endpoint signs in the user with only four claims (`NameIdentifier`, `Email`,
`Name`, `access_token`). The nav menu and home routing page read `AppClaimTypes.AppType`,
`AppClaimTypes.OrganisationId`, `AppClaimTypes.OrganisationName`, and
`AppClaimTypes.DisplayName` — none of which are set.

**Fix required:**
- API login endpoint must query the user's `OrganisationMembership` after validating
  credentials and include `AppType` ("employer"/"recruiter"/"jobseeker"), `OrganisationId`,
  `OrganisationName`, and `OrganisationType` in the `LoginResponse`.
- Web `/account/login` handler maps those fields to `AppClaimTypes.*` claims in the cookie.

Until this is fixed: nav is empty for all users, home page routing does nothing, all
role-specific dashboards are unreachable from the menu.

### BUG-02 · URL mismatches — web pages calling wrong API paths
Several existing pages call API paths that either don't exist or are at the wrong route.
All API calls from the web must include the `/api/v1/` prefix and match the real route.

| Page | Currently calls | Actual API route |
|------|----------------|-----------------|
| MyApplications.razor | `/applications/me` | `/api/v1/applications/mine` |
| JobSeekerProfile.razor | `/jobseeker/profile` | `/api/v1/me/profile` |
| OrganisationProfile.razor | `/org/me/profile` | **No endpoint exists — see API-01** |
| OrganisationTeam.razor | `/org/me/invites`, `/org/invites/accept` | **No endpoints exist — see API-02** |
| ApplicationDetail.razor | `/applications/status-options`, `/applications/{id}/status` | `/api/v1/applications/...` ✓ (prefix missing) |
| EditJob.razor | `/jobs/{id}` (PUT) | **No PUT endpoint exists — see API-03** |
| NewJob.razor | `/jobs` | `/api/v1/jobs` ✓ (prefix missing) |
| JobDetail.razor | `/jobs/{id}` | `/api/v1/jobs/{id}` ✓ (prefix missing) |
| PublicJobs.razor | `/public/jobs` | `/api/v1/public/jobs` ✓ (prefix missing) |

Many pages are missing the `/api/v1/` prefix on every call. This needs a systematic sweep.

---

## Missing API Endpoints

These need to be built before the corresponding UI pages can function.

### API-01 · Organisation profile management
Needed by: `OrganisationProfile.razor`

```
GET  /api/v1/organisations/me/profile     → return org profile for current user's org
PUT  /api/v1/organisations/me/profile     → update org profile
```

No handler exists yet — needs `GetMyOrganisationProfileHandler` and
`UpdateMyOrganisationProfileHandler` in `Aethon.Application/Organisations/`.

### API-02 · Organisation team / invite management
Needed by: `OrganisationTeam.razor`

```
GET  /api/v1/organisations/me/members          → list active members
POST /api/v1/organisations/me/invites          → send invite by email
GET  /api/v1/organisations/me/invites          → list pending invites
DELETE /api/v1/organisations/me/members/{id}   → remove member
POST /api/v1/organisations/invites/accept      → accept invite (by token or logged-in user)
```

No handlers exist yet — needs Organisation Commands/Queries.

### API-03 · Job update and lifecycle
Needed by: `EditJob.razor`, `JobDetail.razor`

```
PUT  /api/v1/jobs/{id}         → update job (title, description, salary, location, etc.)
POST /api/v1/jobs/{id}/publish → transition draft → published
POST /api/v1/jobs/{id}/close   → close job (no more applications)
POST /api/v1/jobs/{id}/archive → archive job
```

No handlers exist for update/lifecycle — needs `UpdateJobHandler`, `PublishJobHandler`, etc.

### API-04 · Job seeker dashboard data
Needed by: `JobSeekerDashboard.razor`

```
GET /api/v1/me/dashboard      → application counts, profile completion %, recent activity
```

Or the dashboard page can assemble this from existing endpoints (`/me/profile`,
`/applications/mine`) — no new endpoint strictly required, but a summary endpoint is cleaner.

### API-05 · Employer/Recruiter dashboard data
Needed by: `EmployerDashboard.razor`, `RecruiterDashboard.razor`

```
GET /api/v1/organisations/me/dashboard   → open job count, pending apps, recent activity
```

### API-06 · Recruiter–company relationship management
Needed by: `Recruiter/Companies/Index.razor`, `Recruiter/Companies/Request.razor`,
`Company/Recruiters/Index.razor`, `Company/Recruiters/Pending.razor`

```
POST /api/v1/recruiter/companies/requests            → recruiter requests partnership
GET  /api/v1/recruiter/companies                     → list company partnerships
GET  /api/v1/company/recruiters                      → list recruiter partnerships
GET  /api/v1/company/recruiters/pending              → list pending requests
POST /api/v1/company/recruiters/{id}/approve         → approve request
POST /api/v1/company/recruiters/{id}/reject          → reject request
```

No handlers exist — needs `OrganisationRecruitmentPartnership` Commands/Queries.

### API-07 · Recruiter job submission/approval workflow
Needed by: `Recruiter/Jobs/*.razor`, `Company/Jobs/PendingApprovals.razor`

```
POST /api/v1/recruiter/jobs                          → recruiter creates job draft
GET  /api/v1/recruiter/jobs                          → list recruiter's submitted jobs
PUT  /api/v1/recruiter/jobs/{id}                     → update recruiter job draft
POST /api/v1/recruiter/jobs/{id}/submit              → submit for company approval
GET  /api/v1/company/jobs/approvals/pending          → list jobs awaiting approval
POST /api/v1/company/jobs/approvals/{id}/approve     → approve recruiter job
POST /api/v1/company/jobs/approvals/{id}/reject      → reject recruiter job
```

No handlers exist for recruiter job workflow.

### API-08 · Public organisation profiles
Needed by: `PublicOrganisation.razor`

```
GET /api/v1/public/organisations/{slug}   → public profile (already exists in FileEndpoints area?)
```

Confirm this is mapped — check `PublicEndpoints` if it exists, otherwise add it.

---

## Existing Pages — Status

### ✅ Working (or fixable with URL prefix only)

| Page | Route | Notes |
|------|-------|-------|
| Login.razor | `/login` | Working ✓ |
| Register.razor | `/register` | Working ✓ |
| PublicJobs.razor | `/jobs` | Fix `/api/v1/` prefix |
| PublicJobDetail.razor | `/jobs/{id}` | Fix `/api/v1/` prefix |
| MyApplications.razor | `/app/applications` | Fix URL to `/api/v1/applications/mine` |
| ApplicationDetail.razor | `/app/applications/{id}` | Fix `/api/v1/` prefix |
| JobList.razor | `/app/jobs` | Fix `/api/v1/` prefix |
| JobDetail.razor | `/app/jobs/{id}` | Fix `/api/v1/` prefix |
| NewJob.razor | `/app/jobs/new` | Fix `/api/v1/` prefix |

### ⚠️ Needs API endpoint built first

| Page | Route | Blocked by |
|------|-------|-----------|
| OrganisationProfile.razor | `/app/organisation/profile` | API-01 |
| OrganisationTeam.razor | `/app/organisation/team` | API-02 |
| EditJob.razor | `/app/jobs/{id}/edit` | API-03 (PUT job) |
| EmployerDashboard.razor | `/app/employer` | API-05 or assemble from existing |
| RecruiterDashboard.razor | `/app/recruiter` | API-05 or assemble from existing |
| JobSeekerDashboard.razor | `/app/jobseeker` | API-04 or assemble from existing |
| JobSeekerProfile.razor | `/app/jobseeker/profile` | Fix URL to `/api/v1/me/profile` |
| Company/Recruiters/Index.razor | `/company/recruiters` | API-06 |
| Company/Recruiters/Pending.razor | `/company/recruiters/pending` | API-06 |
| Recruiter/Companies/Index.razor | `/recruiter/companies` | API-06 |
| Recruiter/Companies/Request.razor | `/recruiter/companies/request` | API-06 |
| Recruiter/Jobs/Index.razor | `/recruiter/jobs` | API-07 |
| Recruiter/Jobs/Create.razor | `/recruiter/jobs/create` | API-07 |
| Recruiter/Jobs/Edit.razor | `/recruiter/jobs/edit/{id}` | API-07 |
| Company/Jobs/PendingApprovals.razor | `/company/jobs/pending-approvals` | API-07 |

### 🆕 Missing pages (endpoint exists, no UI)

| Feature | Suggested route | Endpoint |
|---------|----------------|---------|
| Interview scheduling modal/page | Within ApplicationDetail | `POST /api/v1/applications/{id}/interviews` |
| Application notes panel | Within ApplicationDetail | `POST /api/v1/applications/{id}/notes` |
| Application comments thread | Within ApplicationDetail | `POST /api/v1/applications/{id}/comments` |
| Application timeline view | Within ApplicationDetail | `GET /api/v1/applications/{id}/timeline` |
| Integration / Webhook settings | `/app/organisation/integrations` | `GET/POST /api/v1/integrations/organisations/{id}/webhooks` |

---

## Navigation Gaps

The nav menu is missing links to several existing pages:

| Missing link | For | Where to add |
|-------------|-----|-------------|
| Recruiter partnerships | Employer nav | Under "Organisation team" |
| Pending approvals | Employer nav | Under "Jobs" |
| Company partnerships | Recruiter nav | Under "Organisation team" |
| Integration settings | Employer + Recruiter nav | New "Settings" section |
| Browse jobs | Employer + Recruiter nav | Useful reference |

---

## Suggested Work Order

### Phase 1 — Unblock login and routing (must-do first)
1. Fix **BUG-01**: populate `AppClaimTypes` claims on login (API + Web login endpoint)
2. Fix **BUG-02**: sweep all web pages and fix `/api/v1/` URL prefixes

### Phase 2 — Get employer user fully working
1. Build **API-01** (org profile endpoints + handlers)
2. Build **API-03** (job update + publish/close endpoints + handlers)
3. Fix `OrganisationProfile.razor` URL
4. Fix `EditJob.razor` to call PUT endpoint
5. Wire up job lifecycle buttons in `JobDetail.razor` (publish, close, archive)
6. Build **API-05** (employer dashboard summary) or assemble from existing endpoints
7. Expand `ApplicationDetail.razor` to include notes, comments, timeline, interview scheduling

### Phase 3 — Get job seeker user fully working
1. Fix `JobSeekerProfile.razor` URL (`/api/v1/me/profile`)
2. Fix `MyApplications.razor` URL (`/api/v1/applications/mine`)
3. Build **API-04** (jobseeker dashboard) or assemble from existing endpoints
4. Verify public job browsing and application submission end-to-end

### Phase 4 — Recruiter–company workflows
1. Build **API-06** (partnership management endpoints + handlers)
2. Build **API-07** (recruiter job submission/approval + handlers)
3. Fix all recruiter pages to use correct URLs
4. Fix all company/recruiter relationship pages

### Phase 5 — Polish and missing features
1. Add **integration settings page** (`/app/organisation/integrations`)
2. Add missing nav links (pending approvals, partnerships, integrations)
3. Expand `ApplicationDetail.razor` with full timeline, notes, comments, interview scheduling
4. Add `PublicOrganisation.razor` support if needed

---

## API Surface — Complete Map

### Currently implemented and correctly routed

```
POST   /api/v1/auth/register
POST   /api/v1/auth/login

POST   /api/v1/jobs                              create job
GET    /api/v1/jobs/{id}                         get job
GET    /api/v1/jobs/{id}/applications            applications for job

GET    /api/v1/applications/status-options
POST   /api/v1/applications                      submit application
GET    /api/v1/applications/mine                 my applications (job seeker)
GET    /api/v1/applications/{id}                 get application
GET    /api/v1/applications/{id}/timeline
POST   /api/v1/applications/{id}/status
POST   /api/v1/applications/{id}/notes
POST   /api/v1/applications/{id}/comments
POST   /api/v1/applications/{id}/interviews
POST   /api/v1/applications/{id}/files
GET    /api/v1/applications/{id}/files

GET    /api/v1/me/profile                        candidate profile
PUT    /api/v1/me/profile                        update candidate profile

POST   /api/v1/files                             upload file
GET    /api/v1/files/{id}/download               download file

GET    /api/v1/integrations/organisations/{id}/webhooks
POST   /api/v1/integrations/organisations/{id}/webhooks

GET    /api/v1/public/jobs
GET    /api/v1/public/jobs/{id}
POST   /api/v1/public/jobs/{id}/apply
```

### Not yet implemented (needed for existing pages)

```
GET    /api/v1/organisations/me/profile
PUT    /api/v1/organisations/me/profile
GET    /api/v1/organisations/me/members
POST   /api/v1/organisations/me/invites
GET    /api/v1/organisations/me/invites
POST   /api/v1/organisations/invites/accept
DELETE /api/v1/organisations/me/members/{id}

PUT    /api/v1/jobs/{id}
POST   /api/v1/jobs/{id}/publish
POST   /api/v1/jobs/{id}/close
POST   /api/v1/jobs/{id}/archive

POST   /api/v1/recruiter/companies/requests
GET    /api/v1/recruiter/companies
GET    /api/v1/company/recruiters
GET    /api/v1/company/recruiters/pending
POST   /api/v1/company/recruiters/{id}/approve
POST   /api/v1/company/recruiters/{id}/reject

POST   /api/v1/recruiter/jobs
GET    /api/v1/recruiter/jobs
PUT    /api/v1/recruiter/jobs/{id}
POST   /api/v1/recruiter/jobs/{id}/submit
GET    /api/v1/company/jobs/approvals/pending
POST   /api/v1/company/jobs/approvals/{id}/approve
POST   /api/v1/company/jobs/approvals/{id}/reject
```
