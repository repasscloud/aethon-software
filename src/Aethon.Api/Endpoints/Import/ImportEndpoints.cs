using Aethon.Api.Common;
using Aethon.Application.Import.Commands.ImportJobs;
using Microsoft.AspNetCore.Mvc;

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

        // POST /api/v1/import/jobs/bulk — ingest up to 500 jobs in a single call
        group.MapPost("/jobs/bulk", async (
            HttpContext http,
            [FromServices] ImportJobsHandler handler,
            [FromBody] List<ImportJobDto> dtos,
            CancellationToken ct) =>
        {
            var apiKey = http.Request.Headers[ApiKeyHeader].ToString();
            var result = await handler.HandleBulkAsync(apiKey, dtos, ct);
            return result.ToMinimalApiResult();
        });
    }
}
