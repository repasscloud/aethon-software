# Plan: Admin Compose Email

Send a custom HTML email to any user from the admin/support panel.

---

## Overview

- New admin page `/admin/compose-email` with user picker, subject field, and TinyMCE editor
- New `Email__SupportEmail` setting for the "from" address on admin-sent mail (separate from the transactional `do-not-reply` address)
- Button on the user detail page (`/admin/users/{id}`) that deep-links to the compose page with the recipient pre-filled and locked
- New API endpoint `POST /api/v1/admin/compose-email`

---

## New Setting: `Email__SupportEmail`

Admin-sent mail should come from a human-readable address (e.g. `support@repasscloud.com` or `hello@repasscloud.com`), not from `do-not-reply@...`. This makes it feel like a real message, not an automated one.

Resolution order (same as all other email settings): DB → ENV → empty/misconfigured.

**Key name:** `Email__SupportEmail`

### Files to update

| File | Change |
|---|---|
| `src/Aethon.Application/Abstractions/Settings/SystemSettingKeys.cs` | Add `EmailSupportEmail = "Email__SupportEmail"` under the Email section |
| `src/Aethon.Api/Infrastructure/Email/EmailOptions.cs` | Add `public string SupportEmail { get; set; } = "";` |
| `src/Aethon.Api/Infrastructure/Email/EmailOptionsResolver.cs` | Resolve `SupportEmail` from DB then config fallback |
| `src/Aethon.Api/appsettings.json` | Add `"SupportEmail": ""` under the `Email` block |
| `.env.example` | Add `Email__SupportEmail=` |
| `compose.yaml` | Add `Email__SupportEmail: "${Email__SupportEmail:-}"` |
| `compose.local.yaml` | Add `Email__SupportEmail: "${Email__SupportEmail:-}"` |
| `src/Aethon.Web/Components/Pages/Admin/Settings.razor` | Add "Support From Email" field to the Email settings section, saved as `Email__SupportEmail` |

---

## API Endpoint

### `POST /api/v1/admin/compose-email`

**Auth:** `SuperAdmin`, `Admin`, `Support` roles only

**Request body:**
```json
{
  "userId": "guid",
  "subject": "string (1–200 chars)",
  "htmlBody": "string (non-empty)"
}
```

**Behaviour:**
1. Look up user by `userId` — return 404 if not found
2. Resolve `SupportEmail` from `EmailOptionsResolver` — return 503 if not configured
3. Send email via `IEmailService.SendAsync()` using the `Wrap()` layout from `EmailTemplateService` so it gets the same branded header/footer
4. Write a `SystemLog` entry (category `"AdminEmail"`) recording who sent it, to whom, and the subject
5. Return `200 OK` with `{ "sent": true, "toEmail": "..." }`

**File:** `src/Aethon.Api/Endpoints/Admin/AdminEndpoints.cs` — add near the other admin action endpoints

### Supporting API: user search for the picker

The existing `GET /api/v1/admin/users?search=&page=` endpoint already returns `id`, `email`, `displayName`, `userType`, `organisationName`. This is sufficient for the dropdown — no new endpoint needed.

---

## Blazor Page: `/admin/compose-email`

**File:** `src/Aethon.Web/Components/Pages/Admin/ComposeEmail.razor`

**Route:** `@page "/admin/compose-email"`

**Auth:** Staff only (same as other admin pages)

### Layout

```
┌─────────────────────────────────────────────────────────┐
│  Compose Email                                          │
├─────────────────────────────────────────────────────────┤
│  Recipient                                              │
│  [ Search users...                              ▼ ]     │
│  ← shows: DisplayName · email@... · UserType · OrgName  │
│  (locked with padlock icon when pre-filled via URL)     │
├─────────────────────────────────────────────────────────┤
│  Subject                                                │
│  [ ___________________________________ ]                │
├─────────────────────────────────────────────────────────┤
│  Message                                                │
│  ┌─────────────────────────────────────────────────┐   │
│  │  TinyMCE editor                                 │   │
│  └─────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────┤
│  From: support@repasscloud.com  (resolved, read-only)   │
│                                     [ Send Email ]      │
└─────────────────────────────────────────────────────────┘
```

### Recipient picker behaviour

- Typing in the search box calls `GET /api/v1/admin/users?search={term}&page=1`
- Dropdown shows up to 10 results, each entry displaying:
  - **DisplayName** (bold)
  - `email@address` (muted)
  - Badge for UserType (Company / Recruiter / JobSeeker / Staff)
  - Organisation name if present
- Once selected, the chosen user's name + email is shown in a read-only chip
- An `×` clears the selection (unless locked)

### Locked mode (deep-link from user detail page)

Query string: `/admin/compose-email?userId={guid}`

When `userId` is present in the URL:
- On load, call `GET /api/v1/admin/users/{id}` to fetch user details
- Pre-fill the recipient chip showing their name and email
- Replace the search input with a locked chip (padlock icon, no `×` button)
- This prevents accidentally changing the recipient mid-compose

### TinyMCE

Uses the existing `tinymce-interop.js` already in the project. Same setup as the existing TinyMCE usage elsewhere in the app.

### Send flow

1. Validate: recipient selected, subject non-empty, body non-empty
2. `POST /api/v1/admin/compose-email`
3. On success: show green alert "Email sent to {name} ({email})", clear subject + body, keep recipient (so you can send another)
4. On error: show red alert with the error message

---

## User Detail Page: "Send Aethon Mail" button

**File:** `src/Aethon.Web/Components/Pages/Admin/Users/Detail.razor`

Add a button in the admin actions section (alongside Disable, Reset Password, etc.):

```html
<a href="/admin/compose-email?userId=@_user.Id" class="btn btn-outline-primary">
    <i class="bi bi-envelope"></i> Send Aethon Mail
</a>
```

---

## Nav Menu

**File:** `src/Aethon.Web/Components/Layout/NavMenu.razor`

Add to the staff nav section, after or near "System Logs":

```html
<NavLink class="nav-link aethon-nav-link" href="/admin/compose-email" Match="NavLinkMatch.Prefix">
    <span class="aethon-nav-icon"><i class="bi bi-envelope-paper"></i></span>
    <span>Compose Email</span>
</NavLink>
```

---

## Email template wrapping

The compose endpoint does **not** use a new template — it wraps the admin's raw HTML in the existing `Wrap()` layout from `EmailTemplateService`. This means:

- The branded header (Aethon logo mark + name + subtitle) appears at the top
- The standard footer ("You received this email because...") appears at the bottom
- The admin's composed HTML goes in the middle content area

The `TextBody` sent to MailerSend will be a plain-text strip of the HTML (using a simple tag-stripping regex).

---

## SystemLog entries

Every send is logged under category `"AdminEmail"`:

| Event | Level | Message |
|---|---|---|
| Sent successfully | Info | `Admin email sent to {email} by {adminEmail}` |
| SupportEmail not configured | Warning | `Admin email not sent — Email__SupportEmail is not configured` |
| MailerSend error | Error | `Admin email to {email} rejected by MailerSend (HTTP {status})` |

---

## Implementation order

1. `SystemSettingKeys.cs` — add `EmailSupportEmail`
2. `EmailOptions.cs` — add `SupportEmail` property
3. `EmailOptionsResolver.cs` — resolve `SupportEmail`
4. `appsettings.json`, `.env.example`, `compose.yaml`, `compose.local.yaml` — add env var
5. `AdminEndpoints.cs` — add `POST /api/v1/admin/compose-email`
6. `Settings.razor` — add Support From Email field
7. `ComposeEmail.razor` — new Blazor page
8. `NavMenu.razor` — add nav link
9. `Detail.razor` — add "Send Aethon Mail" button

---

## Open questions for review

1. **Setting name** — `Email__SupportEmail` proposed. Alternative: `Email__FromEmailSupport`. Do you have a preference?
2. **Roles** — Should `Support` role be able to send, or restrict to `Admin`/`SuperAdmin` only?
3. **Plain-text fallback** — Strip HTML tags for the `text` body, or skip it (MailerSend allows html-only)?
4. **Reply-to** — Should the sent email have a reply-to set so replies land somewhere useful, or leave blank?
5. **Audit trail** — Log to `SystemLogs` only, or also write an `ActivityLog` entry against the user?

## Answers to questions for review

1. Use the alernative.
2. Support should be able to send too
3. Whatever you think is the best option
4. The reply to should be the sender email address that is in Email__FromEMailSupport
5. Log to `SystemLogs` and write an `ActivityLog` entry against the user account
6. 