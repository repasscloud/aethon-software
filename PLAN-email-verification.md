# Plan: Email Verification + Settings Management

**Date:** 2026-03-27
**Status:** Decisions resolved — ready to implement

---

## Problem Summary

1. Registration sets `RequiresEmailConfirmation = false` and never sends a verification email.
2. Login does not check `user.EmailConfirmed`, so unverified users can log straight in.
3. No dedicated "check your email" post-registration page exists.
4. Email service credentials live only in ENV VARs / `appsettings.json` — not in the DB alongside Stripe settings.
5. No admin UI for email settings (analogous to `/admin/stripe-products`).
6. Email templates are raw inline strings in endpoint files; no easy way to update them post-deploy.
7. Admin dashboard shows no warning when email is misconfigured.

---

## Scope — What is NOT changing

- Admin override via `POST /api/v1/admin/users/{userId}/verify-email` — keep exactly as-is.
- Admin user detail page `/admin/users/{id}` — keep as-is.
- Stripe settings page — keep as-is (reference implementation we follow).
- Existing password-reset, staff-account, and identity-rejection email flows — keep as-is (they will benefit from the template system automatically).

---

## Decisions Resolved

| # | Decision |
|---|---|
| Manual verification page | **No manual page.** Verification email contains a clickable link; user can also copy/paste the URL into their browser. |
| Existing unverified users | **SQL file at repo root.** Dev team runs it manually before PROD deploy. No automatic migration. |
| Email template editing UI | **Plain `<textarea>` only.** DEV team owns templates. Admin can add/update/delete templates and give them names. No editing by company/recruiter/job-seeker roles ever. Named templates enable future "Send email using [template dropdown]" in action services. |
| `webBaseUrl` in DB | **Yes.** Always DB first → ENV VAR second. If both are null/empty, that is site admin's fault — show warning on dashboard. |

---

## Phase 1 — Email Settings in the Database

### 1.1 Add setting keys

**File:** `src/Aethon.Application/Abstractions/Settings/SystemSettingKeys.cs`

Add a new `Email` nested class (following existing pattern):

```
Email__MailerSendApiKey
Email__FromEmail
Email__FromName
Email__WebBaseUrl
```

### 1.2 Update `EmailOptions` resolution

Create `EmailOptionsResolver` (scoped service) with resolution order:

```
1. SystemSettings DB (via SystemSettingsService, cached)
2. IConfiguration (ENV VAR → appsettings.json)
3. → if still null/empty: treat as misconfigured
```

- `MailerSendEmailService` receives `EmailOptionsResolver` instead of `IOptions<EmailOptions>` directly.
- The resolver merges DB values over the bound config values (DB wins).

**New file:** `src/Aethon.Api/Infrastructure/Email/EmailOptionsResolver.cs`

### 1.3 "Email misconfigured" warning on admin dashboard

**`GET /api/v1/admin/stats`** — add `EmailConfigured: bool` to the DTO.
Logic: resolved `MailerSendApiKey` is not null/empty AND `FromEmail` is not null/empty.

**`Admin/Dashboard.razor`** — add warning card (same yellow style as PendingStripeEvents) when `!stats.EmailConfigured`.

---

## Phase 2 — Admin Settings UI (`/admin/settings`)

### 2.1 New Razor page

**File:** `src/Aethon.Web/Components/Pages/Admin/Settings/Index.razor`

Mirror `StripeProducts/Index.razor` structure:
- Auth: require staff role.
- **Email section** — MailerSendApiKey (masked for non-SuperAdmin), FromEmail, FromName, WebBaseUrl.
- **Email Templates section** (SuperAdmin only):
  - List all templates stored in DB (key-value pairs where key starts with `EmailTemplate__`).
  - Each template has: `Name` (display name), `Subject` (plain text), `Html` (plain textarea).
  - Actions per template: Save, Delete.
  - "Add new template" button: prompts for a name, creates new DB entries.
  - Plain `<textarea>` for HTML body — no editor widget.
  - Shows the available `{{VarName}}` tokens per system template as read-only helper text.
- Individual Save buttons per field.
- Calls existing `GET /api/v1/admin/settings` and `PUT /api/v1/admin/settings/{key}`.
- Delete calls `DELETE /api/v1/admin/settings/{key}` (add this endpoint if it doesn't exist).

### 2.2 Nav link

Add `Admin → Settings` link pointing to `/admin/settings` in the admin nav.

---

## Phase 3 — Email Templates in the Database

### 3.1 Template key scheme

System templates (built-in, non-deletable) use predictable keys:

```
EmailTemplate__Verification__Subject
EmailTemplate__Verification__Html
EmailTemplate__PasswordReset__Subject
EmailTemplate__PasswordReset__Html
EmailTemplate__PasswordResetConfirm__Subject
EmailTemplate__PasswordResetConfirm__Html
EmailTemplate__StaffWelcome__Subject
EmailTemplate__StaffWelcome__Html
EmailTemplate__IdentityRejection__Subject
EmailTemplate__IdentityRejectionHtml
```

Custom (admin-created) templates follow the same pattern with an admin-chosen name slug, e.g.:
```
EmailTemplate__CustomWelcomeCampaign__Subject
EmailTemplate__CustomWelcomeCampaign__Html
```

Add these keys to `SystemSettingKeys.cs` for the system templates. Custom templates are free-form keys discovered by prefix scan.

### 3.2 Template rendering service

**New file:** `src/Aethon.Api/Infrastructure/Email/EmailTemplateService.cs`
**New file:** `src/Aethon.Application/Abstractions/Email/IEmailTemplateService.cs`

```csharp
Task<(string subject, string html)> RenderAsync(string templateName, Dictionary<string, string> vars);
```

- Fetches `EmailTemplate__{templateName}__Subject` and `EmailTemplate__{templateName}__Html` from DB.
- Falls back to hardcoded defaults if DB value is null/empty.
- Variable substitution: replaces `{{VarName}}` tokens.

**Variables per system template:**

| Template name | Variables |
|---|---|
| `Verification` | `{{DisplayName}}`, `{{VerificationUrl}}` |
| `PasswordReset` | `{{DisplayName}}`, `{{ResetUrl}}`, `{{MfaWarning}}` |
| `PasswordResetConfirm` | `{{DisplayName}}` |
| `StaffWelcome` | `{{DisplayName}}`, `{{Email}}`, `{{TempPassword}}`, `{{LoginUrl}}` |
| `IdentityRejection` | `{{DisplayName}}`, `{{RejectionReason}}` |

### 3.3 Default templates (hardcoded fallbacks)

Stored as `private const string` inside `EmailTemplateService`:
- One-column HTML table layout (no external CSS framework).
- Brand colour `#111827` for header/button background, white text.
- Footer: "You received this email because you have an Aethon account. If you didn't create an account, you can safely ignore this email."
- Plain text fallback: strip HTML tags.

### 3.4 Replace existing inline templates

Update four existing email-sending sites to use `IEmailTemplateService.RenderAsync(...)`:
- `AuthEndpoints.cs` — password reset, password reset confirm.
- `AdminEndpoints.cs` — staff welcome, identity rejection.
- Plus the new registration verification email (Phase 4).

---

## Phase 4 — Email Verification Flow

### 4.1 Registration — send verification email

**File:** `AuthEndpoints.cs` — `POST /auth/register`

After user (and org/membership) creation, before returning:
1. Generate `userManager.GenerateEmailConfirmationTokenAsync(user)`.
2. Base64Url-encode the token.
3. Build `verificationUrl = $"{resolvedWebBaseUrl}/verify-email?userId={user.Id}&token={encodedToken}"`.
4. Call `emailTemplateService.RenderAsync("Verification", ...)`.
5. Call `emailService.SendAsync(...)`.
6. Return `RequiresEmailConfirmation = true`.

### 4.2 Login — block unverified users

**File:** `AuthEndpoints.cs` — `POST /auth/login`

After password validation succeeds, check:
```csharp
if (!user.EmailConfirmed)
    return Results.Ok(new LoginResultDto { Succeeded = false, RequiresEmailVerification = true });
```

Add `RequiresEmailVerification: bool` to `LoginResultDto`.

### 4.3 New API endpoint — confirm email

```
POST /auth/verify-email
Body: { userId: string, token: string }
Response: { succeeded: bool, errors: string[] }
Auth: anonymous
```

- Base64Url-decode token.
- `userManager.ConfirmEmailAsync(user, decodedToken)`.

### 4.4 New API endpoint — resend verification email

```
POST /auth/resend-verification
Body: { email: string }
Response: { succeeded: bool }   ← always true to prevent email enumeration
Auth: anonymous
```

- Look up user by email; if not found or already confirmed, return success silently.
- Generate new token, send verification email.

### 4.5 Web — "Check your email" page

**File:** `src/Aethon.Web/Components/Pages/Auth/EmailVerificationPending.razor`
**Route:** `/register/check-email`

- Anonymous page.
- Reads `email` from query string.
- Message: "We've sent a verification link to **{email}**. Click the link in the email to verify your account. If you don't see it, check your spam folder."
- "Resend verification email" button → `POST /auth/resend-verification`.
- "Back to sign in" link.

Registration form navigates to this page on success: `/register/check-email?email={Uri.EscapeDataString(email)}`.

### 4.6 Web — Email verification landing page

**File:** `src/Aethon.Web/Components/Pages/Auth/VerifyEmail.razor`
**Route:** `/verify-email`

- Anonymous page.
- Reads `userId` and `token` from query string.
- On `OnInitializedAsync`: calls `POST /auth/verify-email` automatically.
- **Success state:** "Your email address has been verified. You can now sign in." + "Sign in" button.
- **Failure state:** "This verification link is invalid or has expired." + "Send a new verification link" form (email input → `POST /auth/resend-verification`).

### 4.7 Web — Login redirect for unverified users

In the login component, when API returns `RequiresEmailVerification = true`:
- Navigate to `/register/check-email?email={email}` with a banner: "Please verify your email address before signing in."

---

## Phase 5 — SQL File for Existing Users

**New file:** `migrate-existing-users-email-confirmed.sql` at repo root.

```sql
-- Run ONCE before deploying email verification enforcement to PROD.
-- Marks all existing users as email-confirmed so they are not locked out.
-- Review the WHERE clause before running: adjust if you want to exclude certain users.
UPDATE "AspNetUsers"
SET "EmailConfirmed" = TRUE
WHERE "EmailConfirmed" = FALSE;
```

---

## Phase 6 — Seed Script

**New file:** `seed-email-settings.zsh` — mirrors `seed-stripe-settings.zsh` exactly.

**New file:** `email-settings.env.example`

```env
Email__MailerSendApiKey=mlsn.xxxx
Email__FromEmail=do-not-reply@example.com
Email__FromName=Aethon Software
Email__WebBaseUrl=https://app.example.com

# Optional: seed/override system email template subjects
# EmailTemplate__Verification__Subject=Verify your Aethon account
# EmailTemplate__PasswordReset__Subject=Reset your Aethon password
```

---

## Implementation Order

| # | Phase | Risk | Notes |
|---|---|---|---|
| 1 | `SystemSettingKeys` + `EmailOptionsResolver` | Low | Non-breaking; existing ENV vars still work |
| 2 | Admin `/admin/settings` UI + nav link | Low | Reads/writes existing settings endpoints |
| 3 | Dashboard warning (`EmailConfigured`) | Low | UI-only |
| 4 | `EmailTemplateService` + replace inline templates | Medium | Existing emails change visually |
| 5 | Registration: send verification email | Medium | `RequiresEmailConfirmation` → `true` |
| 6 | Web pages: check-email + verify-email | Medium | New routes, no breaking changes |
| 7 | Login: block unverified users | **High** | Deploy only after steps 5–6 are live |
| 8 | SQL file + seed script | Low | Dev tooling, no code changes |

> **Critical:** Deploy steps 5–6 together, then run `migrate-existing-users-email-confirmed.sql` on any environment with existing users, then deploy step 7.

---

## Files to Create

```
src/Aethon.Api/Infrastructure/Email/EmailOptionsResolver.cs
src/Aethon.Api/Infrastructure/Email/EmailTemplateService.cs
src/Aethon.Application/Abstractions/Email/IEmailTemplateService.cs
src/Aethon.Web/Components/Pages/Admin/Settings/Index.razor
src/Aethon.Web/Components/Pages/Auth/EmailVerificationPending.razor
src/Aethon.Web/Components/Pages/Auth/VerifyEmail.razor
seed-email-settings.zsh
email-settings.env.example
migrate-existing-users-email-confirmed.sql
```

## Files to Modify

```
src/Aethon.Application/Abstractions/Settings/SystemSettingKeys.cs   — add Email__ and EmailTemplate__ keys
src/Aethon.Api/Endpoints/Auth/AuthEndpoints.cs                      — register (send email + RequiresEmailConfirmation=true), login (block unverified), new verify/resend endpoints
src/Aethon.Api/Endpoints/Admin/AdminEndpoints.cs                    — stats DTO (EmailConfigured), replace inline templates, add DELETE /settings/{key} if missing
src/Aethon.Api/Program.cs                                           — register EmailOptionsResolver, IEmailTemplateService/EmailTemplateService
src/Aethon.Web/Components/Pages/Auth/[Login page]                   — handle RequiresEmailVerification response
src/Aethon.Web/Components/Layout/[Nav component]                    — add Settings nav link
```

---

## Out of Scope

- 2FA via email (TOTP is already handled).
- Email open/click tracking.
- Bulk email or notification centre.
- Unsubscribe flow.
- Email preview in admin UI.
- Short alphanumeric verification codes (clickable link only; user can copy/paste URL).
- Action services / "Send email using template" trigger (future feature enabled by named templates).
