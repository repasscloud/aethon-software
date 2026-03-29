---
title: Files
description: Upload and download stored files (resumes, attachments, profile pictures)
weight: 80
---

# Files

All endpoints under `/api/v1/files` require authentication.

Files are stored in the platform's file storage backend and referenced by a `StoredFile` record. The same file ID can be linked to multiple contexts (e.g. a resume, an application attachment).

Upload a file first, then reference the returned `fileId` when linking it to a profile or application.

---

## Upload file

**`POST`** `/api/v1/files`

> 🔒 **Authenticated**

Uploads a file to the platform storage. Returns a `StoredFile` record ID for referencing in other endpoints.

**Content type:** `multipart/form-data`

#### Request body (form fields)

| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `file` | `file` | Yes | The file to upload. Must not be empty. |

#### Response `200 OK`

| Field | Type | Description |
| --- | --- | --- |
| `id` | `string (UUID)` | Stored file ID. Use this in other endpoints. |
| `originalFileName` | `string` | Original filename as uploaded. |
| `contentType` | `string` | Detected MIME type. |
| `lengthBytes` | `integer` | File size in bytes. |
| `uploadedUtc` | `string (ISO 8601)` | When the file was uploaded. |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | File uploaded. |
| `400 Bad Request` | No file provided or file is empty. |
| `401 Unauthorized` | Not authenticated. |

#### Usage flow

1. Upload the file → receive `fileId`.
2. Link to profile as resume: [`POST /me/profile/resume/{fileId}`](03-candidates.md#link-an-uploaded-file-as-resume).
3. Attach to application: [`POST /applications/{id}/files`](05-applications.md#attach-file).

---

## Download file

**`GET`** `/api/v1/files/{fileId}/download`

> 🔒 **Authenticated**

Downloads a stored file. The server enforces access control — callers can only download files they have access to (their own files or files associated with applications they can view).

#### Path parameters

| Parameter | Type | Description |
| --- | --- | --- |
| `fileId` | `UUID` | Stored file ID. |

#### Response `200 OK`

The file content is returned as a binary stream. Response headers include:

| Header | Value |
| --- | --- |
| `Content-Type` | File MIME type (e.g. `application/pdf`) |
| `Content-Disposition` | `attachment; filename="original-filename.pdf"` |

#### Status codes

| Status | Meaning |
| --- | --- |
| `200 OK` | File stream returned. |
| `401 Unauthorized` | Not authenticated. |
| `403 Forbidden` | Caller does not have access to this file. |
| `404 Not Found` | File not found. |
