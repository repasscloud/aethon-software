using System.Text.Json;
using Aethon.Api.Common;
using Aethon.Application.Abstractions.Logging;
using Aethon.Application.Import.Commands.ImportJobs;
using Aethon.Shared.Jobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace Aethon.Api.Endpoints.Import;

public static class ImportEndpoints
{
    private const string ApiKeyHeader = "X-Import-Api-Key";

    public static void MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/import")
            .WithTags("Import");

        // POST /api/v1/import/jobs — ingest a single job from an external feed
        group.MapPost("/jobs", async (
            HttpContext http,
            [FromServices] ImportJobsHandler handler,
            [FromBody] ImportJobDto dto,
            CancellationToken ct) =>
        {
            var apiKey = http.Request.Headers[ApiKeyHeader].ToString();
            var result = await handler.HandleAsync(apiKey, dto, ct);
            return result.ToMinimalApiResult();
        });

        // POST /api/v1/import/jobs/bulk
        //
        // Contract:
        //   Request  — JSON array of ImportJobDto objects (up to 1 000 per call)
        //   Response — always HTTP 200 with BulkImportResponseDto
        //              per-record failures are in response.errors, never in HTTP status
        //
        // Non-200 codes only for:
        //   401 — missing or invalid API key
        //   400 — body is not valid JSON, or is not a JSON array
        group.MapPost("/jobs/bulk", async (
            HttpContext http,
            [FromServices] ImportJobsHandler handler,
            [FromServices] ISystemLogService log,
            [FromServices] IOptions<HttpJsonOptions> jsonOpts,
            CancellationToken ct) =>
        {
            var apiKey = http.Request.Headers[ApiKeyHeader].ToString();

            // Auth check before touching the body
            var authError = await handler.ValidateApiKeyAsync(apiKey, ct);
            if (authError is not null)
                return Results.Json(
                    new { code = "import.unauthorized", message = authError },
                    statusCode: StatusCodes.Status401Unauthorized);

            // Parse body
            JsonElement body;
            try
            {
                using var doc = await JsonDocument.ParseAsync(http.Request.Body, cancellationToken: ct);
                body = doc.RootElement.Clone();
            }
            catch (JsonException ex)
            {
                return Results.BadRequest(new { code = "import.invalid_json", message = ex.Message });
            }

            if (body.ValueKind != JsonValueKind.Array)
                return Results.BadRequest(new
                {
                    code    = "import.not_array",
                    message = "Request body must be a JSON array."
                });

            var opts          = jsonOpts.Value.SerializerOptions;
            var totalReceived = body.GetArrayLength();
            var dtos          = new List<ImportJobDto>(totalReceived);
            var parseErrors   = new List<BulkImportErrorDto>(0);
            var index         = 0;

            // Deserialise element-by-element: one bad record never kills the batch
            foreach (var element in body.EnumerateArray())
            {
                ImportJobDto? dto = null;
                try
                {
                    dto = element.Deserialize<ImportJobDto>(opts);
                    if (dto is null)
                    {
                        parseErrors.Add(new BulkImportErrorDto
                        {
                            Index  = index,
                            Reason = "Record deserialized as null."
                        });
                    }
                }
                catch (JsonException ex)
                {
                    var extId = TryReadString(element, "externalId");
                    var site  = TryReadString(element, "sourceSite");

                    parseErrors.Add(new BulkImportErrorDto
                    {
                        Index      = index,
                        ExternalId = extId,
                        SourceSite = site,
                        Reason     = $"Parse error: {ex.Message}"
                    });

                    await log.WarnAsync(
                        "ImportJobs",
                        $"Batch record [{index}] could not be deserialized and was skipped.",
                        details: JsonSerializer.Serialize(new
                        {
                            batchIndex = index,
                            sourceSite = site,
                            externalId = extId,
                            error      = ex.Message
                        }),
                        ct: ct);
                }

                if (dto is not null)
                    dtos.Add(dto);

                index++;
            }

            // Process whatever successfully parsed (may be empty — that's fine, returns all-zero counts)
            var handlerResult = await handler.HandleBulkAsync(dtos, ct);

            var imported = 0;
            var updated  = 0;
            var skipped  = 0;
            foreach (var r in handlerResult.Succeeded)
            {
                if      (r.WasUpdated)    updated++;
                else if (r.WasDuplicate)  skipped++;
                else                      imported++;
            }

            // Merge handler-level failures into the errors list
            var allErrors = new List<BulkImportErrorDto>(parseErrors);
            foreach (var f in handlerResult.Failed)
            {
                allErrors.Add(new BulkImportErrorDto
                {
                    Index      = -1,
                    ExternalId = f.ExternalId,
                    SourceSite = f.SourceSite,
                    Reason     = f.Reason
                });
            }

            return Results.Ok(new BulkImportResponseDto
            {
                Received = totalReceived,
                Imported = imported,
                Updated  = updated,
                Skipped  = skipped,
                Failed   = allErrors.Count,
                Errors   = allErrors
            });
        });
    }

    private static string? TryReadString(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out var prop) &&
            prop.ValueKind == JsonValueKind.String)
            return prop.GetString();
        return null;
    }
}
