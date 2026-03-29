---
title: Documentation Style Guide
description: Conventions and templates for writing Aethon API docs
weight: 0
---

# Documentation Style Guide

This guide defines the conventions used across all Aethon API documentation. Follow these rules exactly when adding or updating docs so tooling (Hugo, Sphinx, or similar) can process them consistently.

---

## File naming

| Location | Convention | Example |
| --- | --- | --- |
| `_docs/api/` | `NN-group-name.md` — numeric prefix for ordering | `01-auth.md` |
| `_docs/models/` | `noun.md` — singular noun | `enums.md` |
| `_docs/` root | lowercase with hyphens | `style-guide.md` |

---

## Front matter

Every page **must** begin with YAML front matter. Hugo uses this; Sphinx ignores it gracefully.

```yaml
---
title: Human-Readable Page Title
description: One-sentence summary used in search and nav tooltips
weight: 10        # Lower numbers appear first in nav
---
```

---

## Heading hierarchy

```
# Page title (H1) — matches front matter `title`
## Section (H2) — major grouping within the page
### Endpoint name (H3) — one per endpoint
#### Sub-section (H4) — Request / Response / Examples within an endpoint
```

Do not skip levels. Never use more than one H1 per file.

---

## Method + route badge

Write the method and route on a single line using inline code, bolded method:

```markdown
**`POST`** `/api/v1/auth/login`
```

Renders as: **`POST`** `/api/v1/auth/login`

---

## Authentication badge

Use a blockquote immediately after the method line:

```markdown
> 🔓 **Public** — no authentication required
> 🔒 **Authenticated** — valid JWT Bearer token required
> 🛡️ **Admin** — requires role: `SuperAdmin`, `Admin`
> 🛡️ **SuperAdmin** — requires role: `SuperAdmin` only
```

---

## Field tables

Use consistent column order for all request and response tables.

### Request fields

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `email` | `string` | Yes | User email address. Max 320 chars. |
| `name` | `string` | No | Display name. Max 200 chars. |

### Response fields

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `token` | `string` | No | JWT access token. |
| `userId` | `string (UUID)` | No | Authenticated user ID. |

---

## HTTP status codes table

Place immediately after the response field table:

| Status | Meaning |
| --- | --- |
| `200 OK` | Success |
| `400 Bad Request` | Validation error — body contains `errors` map |
| `401 Unauthorized` | Missing or invalid token |
| `403 Forbidden` | Authenticated but insufficient role/permission |
| `404 Not Found` | Resource does not exist or access denied |
| `409 Conflict` | Duplicate resource |
| `422 Unprocessable Entity` | Business rule violation |

---

## Error response shape

All `400` validation errors return a standard RFC 7807 problem details object:

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "fieldName": ["Error message 1", "Error message 2"]
  }
}
```

Business rule failures return:

```json
{
  "code": "domain.error_code",
  "message": "Human-readable explanation."
}
```

---

## Enum values

Reference enum values by their string representation (JSON serialisation uses `JsonStringEnumConverter`). Example: `"WorkplaceType": "Remote"` not `"WorkplaceType": 3`.

Enums are fully documented in [`/models/enums.md`](../models/enums.md).

---

## Adding a new endpoint page

1. Create `_docs/api/NN-group.md` with front matter.
2. Add a one-line entry to [`_docs/README.md`](README.md) under the correct section.
3. Follow the endpoint template below.

### Endpoint template

```markdown
### Endpoint Name

**`METHOD`** `/api/v1/path/{param}`

> 🔒 **Authenticated** — valid JWT Bearer token required

Short description of what this endpoint does and when to use it.

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `param` | `UUID` | The resource identifier. |

#### Query parameters

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `page` | `integer` | No | `1` | Page number. |

#### Request body

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `field` | `string` | Yes | What it does. Max N chars. |

#### Response `200 OK`

| Field | Type | Nullable | Description |
| --- | --- | --- | --- |
| `id` | `string (UUID)` | No | Resource ID. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | Success |
| `404 Not Found` | Resource not found. |
```

---

## What NOT to include

- Internal implementation details (handler class names, EF queries).
- Changelog / history — use git log.
- Dates / version numbers unless part of the API contract.
- Duplicate content — cross-link instead.
