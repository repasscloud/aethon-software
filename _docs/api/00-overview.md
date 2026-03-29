---
title: API Overview
description: Base URL, authentication, request format, pagination, and error handling conventions
weight: 0
---

# API Overview

## Base URL

```
https://<host>/api/v1
```

All routes in this documentation are relative to this base. The version segment (`/v1`) is fixed and will increment only with breaking changes.

---

## Request format

All request bodies must be sent as JSON with the header:

```
Content-Type: application/json
```

File upload endpoints use `multipart/form-data` (see [Files](08-files.md)).

---

## Authentication

Aethon uses **JWT Bearer tokens**. Obtain a token from the login endpoint.

```
Authorization: Bearer <token>
```

Tokens carry the following claims:

| Claim | Description |
| --- | --- |
| `sub` | User ID (UUID) |
| `email` | User email address |
| `role` | One or more roles: `SuperAdmin`, `Admin`, `Support` |
| `userType` | Account type: `JobSeeker`, `Company`, `RecruiterAgency` |

Token lifetime is controlled by the `Jwt:ExpiryMinutes` server setting.

### Authentication states

| Icon | Label | Meaning |
| --- | --- | --- |
| 🔓 | **Public** | No token required. Anyone can call. |
| 🔒 | **Authenticated** | Valid JWT required. Any account type. |
| 🛡️ | **Admin** | Roles: `SuperAdmin`, `Admin`, or `Support` |
| 🛡️⬆️ | **SuperAdmin** | Role: `SuperAdmin` only |

---

## Enum serialisation

All enum fields are serialised as **strings**, not integers.

```json
{ "workplaceType": "Remote" }
```

not:

```json
{ "workplaceType": 3 }
```

See [Enums](../models/enums.md) for all values.

---

## Pagination

Endpoints that return lists accept these query parameters:

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `page` | `integer` | `1` | Page number (1-based). |
| `pageSize` | `integer` | varies | Items per page. Max varies by endpoint. |

Paginated responses include:

| Field | Type | Description |
| --- | --- | --- |
| `items` | `array` | The page of results. |
| `page` | `integer` | Current page number. |
| `pageSize` | `integer` | Items per page requested. |
| `totalCount` | `integer` | Total matching records. |
| `totalPages` | `integer` | Total pages available. |

---

## Errors

### Validation errors — `400 Bad Request`

Returned when request data fails validation. Shape follows RFC 7807:

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "email": ["'Email' is not a valid email address."],
    "password": ["'Password' must be at least 12 characters."]
  }
}
```

### Business rule errors — `400` / `409` / `422`

```json
{
  "code": "applications.age_group_ineligible",
  "message": "This role does not accept applications from school leavers (16–18)."
}
```

### Standard HTTP status codes

| Status | When |
| --- | --- |
| `200 OK` | Successful operation |
| `204 No Content` | Successful deletion with no body |
| `400 Bad Request` | Validation failure or business rule violation |
| `401 Unauthorized` | Missing, expired, or invalid JWT |
| `403 Forbidden` | Valid JWT but insufficient role/permission |
| `404 Not Found` | Resource not found or access denied (intentionally ambiguous) |
| `409 Conflict` | Duplicate resource (e.g., duplicate application) |
| `402 Payment Required` | Insufficient billing credits for the operation |
| `503 Service Unavailable` | External service (email, Stripe) unavailable |

---

## CORS

CORS is configured server-side. Consult deployment configuration for allowed origins.

---

## Date format

All timestamps are ISO 8601 UTC:

```
"publishedUtc": "2026-03-21T09:00:00Z"
```

`DateOnly` fields (birth month/year) are represented as separate integer fields, not date strings, to avoid full DOB capture.

---

## IDs

All resource IDs are **UUID v4** strings:

```
"id": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

---

## Correlation IDs

Every response includes a correlation ID header for tracing:

```
X-Correlation-Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6
```

Include this value when reporting issues.
