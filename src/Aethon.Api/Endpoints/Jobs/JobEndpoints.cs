using Aethon.Api.Common;
using Aethon.Application.Abstractions.Syndication;
using Aethon.Shared.Utilities;
using Aethon.Application.Applications.Queries.GetApplicationsForJob;
using Aethon.Application.Jobs.Commands.CloseJob;
using Aethon.Application.Jobs.Commands.CreateJob;
using Aethon.Application.Jobs.Commands.PublishJob;
using Aethon.Application.Jobs.Commands.ReturnJobToDraft;
using Aethon.Application.Jobs.Commands.UpdateJob;
using Aethon.Application.Jobs.Queries.GetJobById;
using Aethon.Application.Jobs.Queries.GetMyOrgJobs;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Endpoints.Jobs;

public static class JobEndpoints
{
    public static void MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/jobs")
            .RequireAuthorization()
            .WithTags("Jobs");

        group.MapPost(string.Empty, async (
            [FromServices] CreateJobHandler handler,
            HttpContext httpContext,
            CreateJobCommand command,
            CancellationToken ct) =>
        {
            var validation = await httpContext.ValidateAsync(command, ct);
            if (validation is not null)
            {
                return validation;
            }

            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        });

        group.MapGet("/{jobId:guid}", async (
            [FromServices] GetJobByIdHandler handler,
            Guid jobId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(
                new GetJobByIdQuery
                {
                    JobId = jobId
                },
                ct);

            return result.ToMinimalApiResult();
        });

        // GET /api/v1/jobs/my-org  — must be before /{jobId:guid} to avoid routing conflict
        group.MapGet("/my-org", async (
            [FromServices] GetMyOrgJobsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.ToMinimalApiResult();
        });

        group.MapPut("/{jobId:guid}", async (
            [FromServices] UpdateJobHandler handler,
            [FromServices] IGoogleIndexingService indexing,
            [FromServices] IConfiguration config,
            [FromServices] AethonDbContext db,
            HttpContext ctx,
            Guid jobId,
            UpdateJobRequestDto request,
            CancellationToken ct) =>
        {
            var validation = await ctx.ValidateAsync(request, ct);
            if (validation is not null) return validation;

            var result = await handler.HandleAsync(jobId, request, ct);

            // Fire Google Indexing UPDATE if job is published
            if (result.Succeeded)
            {
                var isPublished = await db.Jobs.AsNoTracking()
                    .AnyAsync(j => j.Id == jobId && j.Status == Aethon.Shared.Enums.JobStatus.Published, ct);
                if (isPublished)
                {
                    var baseUrl = config["Email:WebBaseUrl"] ?? config["ApiBaseUrl"] ?? "";
                    await indexing.NotifyUpdatedAsync(jobId, JobUrlHelper.BuildPublicUrl(baseUrl, jobId), ct);
                }
            }

            return result.ToMinimalApiResult();
        });

        group.MapPost("/{jobId:guid}/publish", async (
            [FromServices] PublishJobHandler handler,
            [FromServices] IGoogleIndexingService indexing,
            [FromServices] IConfiguration config,
            Guid jobId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(jobId, ct);

            if (result.Succeeded)
            {
                var baseUrl = config["Email:WebBaseUrl"] ?? config["ApiBaseUrl"] ?? "";
                await indexing.NotifyPublishedAsync(jobId, JobUrlHelper.BuildPublicUrl(baseUrl, jobId), ct);
            }

            return result.ToMinimalApiResult();
        });

        group.MapPost("/{jobId:guid}/close", async (
            [FromServices] CloseJobHandler handler,
            [FromServices] IGoogleIndexingService indexing,
            [FromServices] IConfiguration config,
            Guid jobId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(jobId, ct);

            if (result.Succeeded)
            {
                var baseUrl = config["Email:WebBaseUrl"] ?? config["ApiBaseUrl"] ?? "";
                await indexing.NotifyRemovedAsync(jobId, JobUrlHelper.BuildPublicUrl(baseUrl, jobId), ct);
            }

            return result.ToMinimalApiResult();
        });

        group.MapPost("/{jobId:guid}/return-to-draft", async (
            [FromServices] ReturnJobToDraftHandler handler,
            Guid jobId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(jobId, ct);
            return result.ToMinimalApiResult();
        });

        group.MapGet("/{jobId:guid}/applications", async (
            [FromServices] GetApplicationsForJobHandler handler,
            Guid jobId,
            ApplicationStatus? status = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var result = await handler.HandleAsync(
                new GetApplicationsForJobQuery
                {
                    JobId = jobId,
                    Status = status,
                    Page = page,
                    PageSize = pageSize
                },
                ct);

            return result.ToMinimalApiResult();
        });
    }
}
