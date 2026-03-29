---
title: Authentication
description: Register, login, two-factor authentication, password reset, email verification, MFA setup, and LinkedIn OAuth
weight: 10
---

# Authentication

All authentication endpoints are under `/api/v1/auth`.

Most are public (no token required). MFA management and password change require an active session.

---

## Register

**`POST`** `/api/v1/auth/register`

> 🔓 **Public**

Creates a new user account. Three registration types are supported: `jobseeker`, `company`, and `recruiter`. Company and recruiter registrations also create an Organisation record.

After registration, a verification email is sent. The account can log in once the email is confirmed.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `email` | `string` | Yes | Email address. Max 320 chars. Must be a valid email. |
| `password` | `string` | Yes | Password. Minimum 12 characters. |
| `confirmPassword` | `string` | Yes | Must match `password`. |
| `firstName` | `string` | Yes | First name. Max 100 chars. |
| `lastName` | `string` | Yes | Last name. Max 100 chars. |
| `registrationType` | `string` | Yes | One of: `jobseeker`, `company`, `recruiter`. |
| `organisationName` | `string` | Cond. | Required when `registrationType` is `company` or `recruiter`. Max 250 chars. |
| `ageConfirmed` | `boolean` | Cond. | Required `true` when `registrationType` is `jobseeker`. Confirms the user is 16 or older. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `succeeded` | `boolean` | Always `true` on success. |
| `requiresEmailConfirmation` | `boolean` | Always `true` — user must verify email before logging in. |
| `email` | `string` | Registered email address. |
| `displayName` | `string` | Derived full name. |
| `registrationType` | `string` | The registration type used. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Account created. Verification email sent. |
| `400 Bad Request` | Validation failure (see errors map). Includes duplicate email. |

---

## Login

**`POST`** `/api/v1/auth/login`

> 🔓 **Public**

Authenticates with email and password. Returns a JWT on success. If the account has 2FA enabled, returns a short-lived `twoFactorTicket` instead — complete login via [Verify 2FA](#verify-2fa).

Updates `LastLoginUtc` on every successful login.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `email` | `string` | Yes | Registered email address. |
| `password` | `string` | Yes | Account password. |

#### Response `200 OK`

The response shape varies based on account state:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `token` | `string` | Yes | JWT Bearer token. Null if 2FA required or email unverified. |
| `userId` | `string (UUID)` | Yes | Authenticated user ID. |
| `email` | `string` | No | User email. |
| `displayName` | `string` | Yes | Full display name. |
| `appType` | `string` | Yes | Account type: `jobseeker`, `employer`, `recruiter`, `admin`, `superadmin`, `support`. |
| `isSuperAdmin` | `boolean` | Yes | `true` if SuperAdmin role. |
| `isAdmin` | `boolean` | Yes | `true` if Admin role. |
| `isSupport` | `boolean` | Yes | `true` if Support role. |
| `mustChangePassword` | `boolean` | Yes | `true` if password change is forced on next login. |
| `mustEnableMfa` | `boolean` | Yes | `true` if MFA setup is forced. Only set when 2FA is not yet enabled. |
| `organisationId` | `string (UUID)` | Yes | Active organisation ID. Null for job seekers. |
| `organisationName` | `string` | Yes | Organisation display name. |
| `organisationType` | `string` | Yes | `company` or `recruiter`. |
| `companyRole` | `string` | Yes | Role within a company organisation. |
| `recruiterRole` | `string` | Yes | Role within a recruiter organisation. |
| `isOrganisationOwner` | `boolean` | Yes | `true` if owner of the active organisation. |
| `requiresTwoFactor` | `boolean` | Yes | `true` when 2FA must be completed. See `twoFactorTicket`. |
| `twoFactorTicket` | `string` | Yes | Short-lived ticket for use with `/auth/verify-2fa`. |
| `requiresEmailVerification` | `boolean` | Yes | `true` when email has not been confirmed. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Login successful (or 2FA / email verification step needed — check flags). |
| `400 Bad Request` | Invalid email or password. |

---

## Verify 2FA

**`POST`** `/api/v1/auth/verify-2fa`

> 🔓 **Public**

Completes a login where 2FA is enabled. Requires the `twoFactorTicket` returned by the `/login` endpoint and a current TOTP code from the user's authenticator app.

Returns the same response shape as [Login](#login) on success.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `twoFactorTicket` | `string` | Yes | Short-lived ticket from the `/login` response. |
| `code` | `string` | Yes | 6-digit TOTP code from authenticator app. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | 2FA verified. Full login response returned. |
| `400 Bad Request` | Invalid or expired ticket, or incorrect TOTP code. |

---

## Change password

**`POST`** `/api/v1/auth/change-password`

> 🔒 **Authenticated**

Changes the current user's password. Requires the existing password. Clears `MustChangePassword` flag if set.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `currentPassword` | `string` | Yes | The user's existing password. |
| `newPassword` | `string` | Yes | New password. Minimum 12 characters. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `message` | `string` | Confirmation message. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Password changed. |
| `400 Bad Request` | Current password incorrect, or new password fails policy. |
| `401 Unauthorized` | Not authenticated. |

---

## Forgot password

**`POST`** `/api/v1/auth/forgot-password`

> 🔓 **Public**

Sends a password reset email. Always returns `200` regardless of whether the email exists (prevents user enumeration).

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `email` | `string` | Yes | Email address to send reset link to. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `message` | `string` | Generic confirmation (does not confirm email existence). |

---

## Check reset password MFA requirement

**`GET`** `/api/v1/auth/reset-password/check`

> 🔓 **Public**

Returns whether the account associated with the email has 2FA enabled. Used by the frontend to decide whether to show a TOTP field on the password reset form.

#### Query parameters

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `email` | `string` | Yes | Account email to check. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `requiresMfa` | `boolean` | `true` if the account has 2FA enabled. |

---

## Reset password

**`POST`** `/api/v1/auth/reset-password`

> 🔓 **Public**

Completes a password reset using the token from the reset email. If the account has 2FA enabled, the current TOTP code is also required.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `email` | `string` | Yes | Account email. |
| `token` | `string` | Yes | Reset token from the email link. |
| `newPassword` | `string` | Yes | New password. Minimum 12 characters. |
| `totpCode` | `string` | Cond. | Required if account has 2FA enabled. 6-digit code. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `message` | `string` | Confirmation message. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Password reset successfully. |
| `400 Bad Request` | Invalid token, wrong TOTP code, or password policy failure. |

---

## Verify email

**`POST`** `/api/v1/auth/verify-email`

> 🔓 **Public**

Confirms a user's email address using the token from the verification email. Must be completed before the account can log in.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `userId` | `string (UUID)` | Yes | The user's ID. |
| `token` | `string` | Yes | Verification token from the email link. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `succeeded` | `boolean` | `true` on success. |
| `alreadyConfirmed` | `boolean` | `true` if the email was already confirmed previously. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Email confirmed (or was already confirmed). |
| `400 Bad Request` | Invalid token or user not found. |

---

## Resend verification email

**`POST`** `/api/v1/auth/resend-verification`

> 🔓 **Public**

Resends the email verification link. Rate-limited server-side.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `email` | `string` | Yes | Account email to resend to. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `message` | `string` | Confirmation (generic — does not confirm email existence). |

---

## MFA — get status

**`GET`** `/api/v1/auth/mfa/status`

> 🔒 **Authenticated**

Returns the current 2FA enabled state for the authenticated user.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `twoFactorEnabled` | `boolean` | Whether 2FA is currently active. |

---

## MFA — get setup details

**`GET`** `/api/v1/auth/mfa/setup`

> 🔒 **Authenticated**

Returns the authenticator key and a QR code URI for registering a new TOTP app. Call [Reset authenticator key](#mfa--reset-authenticator-key) first to regenerate if needed.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `authenticatorUri` | `string` | `otpauth://` URI for QR code generation. |
| `key` | `string` | Raw base-32 authenticator key for manual entry. |
| `qrCodeBase64` | `string` | Base64-encoded PNG QR code image. |
| `twoFactorEnabled` | `boolean` | Current 2FA enabled state. |

---

## MFA — reset authenticator key

**`POST`** `/api/v1/auth/mfa/setup/reset-key`

> 🔒 **Authenticated**

Regenerates the authenticator secret key. The user must re-scan the QR code in their authenticator app. Existing tokens will stop working immediately.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `message` | `string` | Confirmation message. |

---

## MFA — enable 2FA

**`POST`** `/api/v1/auth/mfa/setup`

> 🔒 **Authenticated**

Enables 2FA by verifying a TOTP code from the newly configured authenticator app. The key must first be retrieved from [Get setup details](#mfa--get-setup-details).

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `code` | `string` | Yes | 6-digit TOTP code from the authenticator app. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `message` | `string` | Confirmation that 2FA is now enabled. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | 2FA enabled. |
| `400 Bad Request` | Invalid TOTP code. |

---

## MFA — disable 2FA

**`DELETE`** `/api/v1/auth/mfa`

> 🔒 **Authenticated**

Disables 2FA for the current account. Requires no body. If `MustEnableMfa` is set on the account, this will be re-enforced at next login.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `message` | `string` | Confirmation that 2FA is now disabled. |

---

## LinkedIn connect

**`GET`** `/api/v1/auth/linkedin/connect`

> 🔓 **Public** *(token passed as query parameter)*

Initiates the LinkedIn OAuth flow for a job seeker to connect their LinkedIn profile. Redirects the browser to the LinkedIn authorization page.

#### Query parameters

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `token` | `string` | Yes | The user's valid JWT Bearer token (passed here as the browser cannot set headers on a redirect). |

#### Response

`302 Found` — redirects to LinkedIn authorization URL.

LinkedIn calls back to the configured return URL, which completes the connection and populates `LinkedInId` and `LinkedInVerifiedAt` on the job seeker profile.
