---
title: Billing
description: Stripe-powered billing for job posting credits, verification tiers, and add-ons
weight: 70
---

# Billing

All billing endpoints are under `/api/v1/billing`. All require authentication unless noted.

Aethon uses **Stripe** for payment processing. Checkout sessions are created server-side; the client is redirected to a Stripe-hosted checkout page. Credits and verification tiers are updated automatically via Stripe webhooks.

---

## Display prices

**`GET`** `/api/v1/billing/display-prices`

> 🔓 **Public**

Returns all configured display prices for UI presentation (pricing pages, etc.). These are marketing values — actual amounts charged are defined in Stripe.

#### Response `200 OK`

Dictionary of price key strings to display values:

```json
{
  "JobPostingStandard": "49.00",
  "JobPostingPremium": "149.00",
  "VerificationStandard": "99.00"
}
```

---

## Get my credits

**`GET`** `/api/v1/billing/me/credits`

> 🔒 **Authenticated** — company or recruiter member

Returns the active job posting credit balances for the caller's organisation.

#### Response `200 OK`

Array of credit balance items:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `creditType` | `CreditType` | No | Type of credit (e.g. `JobPostingStandard`, `JobPostingPremium`). |
| `source` | `CreditSource` | No | How the credits were acquired (e.g. `Stripe`, `LaunchPromotion`, `AdminGrant`). |
| `quantityRemaining` | `integer` | No | Credits remaining on this batch. |
| `quantityOriginal` | `integer` | No | Original credit quantity. |
| `expiresAt` | `string (ISO 8601)` | Yes | Expiry date. Null if no expiry. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Credit balance returned. |
| `404 Not Found` | No organisation found for the caller. |

---

## Verification checkout

**`POST`** `/api/v1/billing/verification/checkout`

> 🔒 **Authenticated** — company or recruiter member

Creates a Stripe checkout session for purchasing a verification tier upgrade (`Standard` or `Enhanced`).

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `tier` | `string` | Yes | `standard` or `enhanced`. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `checkoutUrl` | `string` | Stripe-hosted checkout page URL. Redirect the user here. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Checkout URL returned. |
| `400 Bad Request` | Invalid tier value. |

---

## Verification + credits bundle checkout

**`POST`** `/api/v1/billing/verification/bundle-checkout`

> 🔒 **Authenticated** — company or recruiter member

Creates a Stripe checkout session for a bundle purchase that includes a verification tier upgrade and a job posting credit pack.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `tier` | `string` | Yes | `standard` or `enhanced`. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `checkoutUrl` | `string` | Stripe-hosted checkout page URL. |

---

## Credits checkout

**`POST`** `/api/v1/billing/credits/checkout`

> 🔒 **Authenticated** — company or recruiter member

Creates a Stripe checkout session for purchasing job posting or sticky/add-on credits.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `priceKey` | `string` | Yes | Stripe price key identifier configured in system settings. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `checkoutUrl` | `string` | Stripe-hosted checkout page URL. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Checkout URL returned. |
| `400 Bad Request` | Price key not found or not configured. |

---

## Job publish checkout

**`POST`** `/api/v1/billing/job-publish-checkout`

> 🔒 **Authenticated**

Attempts to publish a job. If the organisation has available credits, the job is published immediately and no Stripe session is created. If credits are exhausted, returns a Stripe checkout URL to purchase more.

This is the recommended flow for the "Publish" button on the job form — call this endpoint first and branch on whether `published` is `true` or a `checkoutUrl` is returned.

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `jobId` | `string (UUID)` | Yes | Job to publish. |
| `postingTier` | `JobPostingTier` | Yes | `Standard` or `Premium`. |

#### Response `200 OK`

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `published` | `boolean` | No | `true` if the job was published immediately using existing credits. |
| `checkoutUrl` | `string` | Yes | Stripe checkout URL. Only present when `published = false`. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Job published or checkout URL returned. |
| `400 Bad Request` | Job not found or invalid status for publishing. |

---

## Billing portal

**`GET`** `/api/v1/billing/portal`

> 🔒 **Authenticated** — company or recruiter member

Returns a URL for the Stripe Customer Portal where the organisation can manage subscriptions, view invoices, and update payment methods.

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `url` | `string` | Stripe-hosted portal URL. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Portal URL returned. |
| `400 Bad Request` | Organisation not found or Stripe customer not linked. |
