using System.Text.Json;
using Aethon.Api.Common;
using Aethon.Application.Abstractions.Logging;
using Aethon.Application.Import.Commands.ImportJobs;
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

        // POST /api/v1/import/jobs/bulk — ingest up to 500 jobs in a single call.
        //
        // The body is parsed element-by-element so that a single malformed record
        // (e.g. an unrecognised enum value) is skipped and logged rather than
        // causing the entire batch to be rejected.
        group.MapPost("/jobs/bulk", async (
            HttpContext http,
            [FromServices] ImportJobsHandler handler,
            [FromServices] ISystemLogService log,
            [FromServices] IOptions<HttpJsonOptions> jsonOpts,
            CancellationToken ct) =>
        {
            var apiKey = http.Request.Headers[ApiKeyHeader].ToString();

            // Parse the raw body so we can iterate records individually.
            JsonElement body;
            try
            {
                using var doc = await JsonDocument.ParseAsync(http.Request.Body, cancellationToken: ct);
                body = doc.RootElement.Clone();
            }
            catch (JsonException ex)
            {
                return Results.BadRequest(new { error = "Invalid JSON body.", detail = ex.Message });
            }

            if (body.ValueKind != JsonValueKind.Array)
                return Results.BadRequest(new { error = "Request body must be a JSON array." });

            var opts = jsonOpts.Value.SerializerOptions;
            var totalElements = body.GetArrayLength();
            var dtos = new List<ImportJobDto>(totalElements);
            var parseFailures = new List<string>();
            var index = 0;

            foreach (var element in body.EnumerateArray())
            {
                try
                {
                    var dto = element.Deserialize<ImportJobDto>(opts);
                    if (dto is not null)
                        dtos.Add(dto);
                    else
                        parseFailures.Add($"[{index}] deserialized as null");
                }
                catch (JsonException ex)
                {
                    var rawSnippet = element.GetRawText();
                    if (rawSnippet.Length > 200) rawSnippet = rawSnippet[..200] + "…";

                    var failureDetail = $"[{index}] {ex.Message} | raw: {rawSnippet}";
                    parseFailures.Add(failureDetail);

                    await log.WarnAsync(
                        "ImportJobs",
                        $"Batch record at index {index} could not be deserialized and was skipped.",
                        details: JsonSerializer.Serialize(new
                        {
                            batchIndex       = index,
                            exceptionMessage = ex.Message,
                            raw              = element.GetRawText()
                        }),
                        ct: ct);
                }

                index++;
            }

            if (dtos.Count == 0)
            {
                var summary = parseFailures.Count > 0
                    ? $"All {totalElements} record(s) failed to deserialize. First failure: {parseFailures[0]}"
                    : "No jobs provided.";
                return Results.BadRequest(new { code = "import.empty", message = summary });
            }

            var result = await handler.HandleBulkAsync(apiKey, dtos, ct);

            // Surface any parse failures alongside the normal result so CI logs show them
            if (parseFailures.Count > 0 && result.Succeeded)
            {
                var value = result.Value!;
                return Results.Ok(new
                {
                    imported         = value.Count(r => !r.WasDuplicate && !r.WasUpdated),
                    updated          = value.Count(r => r.WasUpdated),
                    duplicatesSkipped = value.Count(r => r.WasDuplicate),
                    parseFailures    = parseFailures.Count,
                    parseFailureDetails = parseFailures
                });
            }

            return result.ToMinimalApiResult();
        });
    }
}
