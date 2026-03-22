using Aethon.Api.Common;
using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Candidates.Commands.TriggerResumeAnalysis;
using Aethon.Application.Candidates.Commands.UpsertMyCandidateProfile;
using Aethon.Application.Candidates.Queries.GetMyCandidateProfile;
using Aethon.Application.Candidates.Queries.GetResumeAnalysis;
using Aethon.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Endpoints.Candidates;

public static class CandidateEndpoints
{
    public static void MapCandidateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/me")
            .RequireAuthorization()
            .WithTags("Candidates");

        group.MapGet("/profile", async (
            [FromServices] GetMyCandidateProfileHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetMyCandidateProfileQuery(), ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/me/resumes/{resumeId}/analysis
        group.MapGet("/resumes/{resumeId:guid}/analysis", async (
            [FromServices] GetResumeAnalysisHandler handler,
            Guid resumeId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(resumeId, ct);
            return result.ToMinimalApiResult();
        });

        // POST /api/v1/me/resumes/{resumeId}/analysis/trigger
        group.MapPost("/resumes/{resumeId:guid}/analysis/trigger", async (
            [FromServices] TriggerResumeAnalysisHandler handler,
            Guid resumeId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(resumeId, ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/me/profile/check-slug?slug=...
        group.MapGet("/profile/check-slug", async (
            AethonDbContext db,
            ICurrentUserAccessor currentUser,
            string slug,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(slug))
                return Results.BadRequest(new { available = false, message = "Slug is required." });

            var normalised = slug.Trim().ToLowerInvariant();
            var taken = await db.JobSeekerProfiles
                .AnyAsync(p => p.Slug == normalised && p.UserId != currentUser.UserId, ct);

            return Results.Ok(new { available = !taken, slug = normalised });
        });

        group.MapPut("/profile", async (
            [FromServices] UpsertMyCandidateProfileHandler handler,
            HttpContext httpContext,
            UpsertMyCandidateProfileCommand command,
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
    }
}
