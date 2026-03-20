using Aethon.Api.Common;
using Aethon.Application.Applications.Queries.GetApplicationsForJob;
using Aethon.Application.Jobs.Commands.CreateJob;
using Aethon.Application.Jobs.Queries.GetJobById;
using Aethon.Shared.Enums;
using Microsoft.AspNetCore.Mvc;

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
