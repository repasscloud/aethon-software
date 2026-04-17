using Aethon.Api.Common;
using Aethon.Application.RecruiterJobs.Commands.CreateRecruiterJobDraft;
using Aethon.Application.RecruiterJobs.Commands.SubmitRecruiterJobForApproval;
using Aethon.Application.RecruiterJobs.Commands.UpdateRecruiterJobDraft;
using Aethon.Application.RecruiterJobs.Queries.GetRecruiterJobs;
using Aethon.Shared.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace Aethon.Api.Endpoints.RecruiterJobs;

public static class RecruiterJobEndpoints
{
    public static void MapRecruiterJobEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/recruiter/jobs")
            .RequireAuthorization()
            .WithTags("RecruiterJobs");

        group.MapGet(string.Empty, async (
            [FromServices] GetRecruiterJobsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost(string.Empty, async (
            [FromServices] CreateRecruiterJobDraftHandler handler,
            HttpContext ctx,
            RecruiterCreateJobDraftDto request,
            CancellationToken ct) =>
        {
            var validation = await ctx.ValidateAsync(request, ct);
            if (validation is not null) return validation;

            var result = await handler.HandleAsync(request, ct);
            return result.ToMinimalApiResult();
        });

        group.MapPut("/{jobId:guid}", async (
            [FromServices] UpdateRecruiterJobDraftHandler handler,
            HttpContext ctx,
            Guid jobId,
            RecruiterUpdateJobDraftDto request,
            CancellationToken ct) =>
        {
            var validation = await ctx.ValidateAsync(request, ct);
            if (validation is not null) return validation;

            var result = await handler.HandleAsync(jobId, request, ct);
            return result.ToMinimalApiResult();
        });

        group.MapPost("/{jobId:guid}/submit", async (
            [FromServices] SubmitRecruiterJobForApprovalHandler handler,
            Guid jobId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(jobId, ct);
            return result.ToMinimalApiResult();
        });
    }
}
