---
title: Integrations
description: Webhook subscriptions for real-time event notifications
weight: 140
---

# Integrations

All endpoints under `/api/v1/integrations` require authentication.

Aethon supports outbound webhooks that fire HTTP POST requests to a configured endpoint URL when platform events occur. Webhooks are scoped to an organisation and secured with a shared secret (HMAC signature).

---

## List webhook subscriptions

**`GET`** `/api/v1/integrations/organisations/{organisationId}/webhooks`

> 🔒 **Authenticated**

Returns all webhook subscriptions for the specified organisation.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `organisationId` | `UUID` | Organisation ID. |

#### Response `200 OK`

Array of `WebhookSubscriptionDto`:

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Subscription ID. |
| `organisationId` | `string (UUID)` | No | Owning organisation ID. |
| `name` | `string` | No | Friendly name for this subscription. |
| `endpointUrl` | `string` | No | URL that receives webhook POST requests. |
| `secret` | `string` | No | HMAC secret used to sign payloads. |
| `isActive` | `boolean` | No | Whether the subscription is active. |
| `events` | `string[]` | No | List of event names this subscription is subscribed to. |

---

## Create webhook subscription

**`POST`** `/api/v1/integrations/organisations/{organisationId}/webhooks`

> 🔒 **Authenticated**

Creates a new webhook subscription for the specified organisation.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `organisationId` | `UUID` | Organisation ID. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `name` | `string` | Yes | Friendly name for this subscription. |
| `endpointUrl` | `string` | Yes | HTTPS URL that will receive webhook events. |
| `secret` | `string` | Yes | Shared secret for HMAC payload signing. Keep this confidential. |
| `events` | `string[]` | Yes | Event names to subscribe to. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `id` | `string (UUID)` | New subscription ID. |
| `organisationId` | `string (UUID)` | Organisation ID. |
| `name` | `string` | Subscription name. |
| `endpointUrl` | `string` | Endpoint URL. |
| `isActive` | `boolean` | Active status (always `true` on creation). |
| `events` | `string[]` | Subscribed events. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Subscription created. |
| `400 Bad Request` | Validation failed (e.g. invalid URL, no events). |
| `401 Unauthorized` | Not authenticated. |

#### Webhook payload format

When a subscribed event fires, Aethon sends a POST request to the configured `endpointUrl`:

```
POST {endpointUrl}
Content-Type: application/json
X-Aethon-Event: {eventName}
X-Aethon-Signature: sha256={hmac-signature}
```

Verify the signature by computing `HMAC-SHA256` of the raw request body using your `secret` and comparing it to the `X-Aethon-Signature` header value.
