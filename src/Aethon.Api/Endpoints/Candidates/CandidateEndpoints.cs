using Aethon.Api.Common;
using Aethon.Application.Candidates.Commands.AddCandidateResume;
using Aethon.Application.Candidates.Commands.RemoveCandidateResume;
using Aethon.Application.Candidates.Commands.SetDefaultCandidateResume;
using Aethon.Application.Candidates.Commands.UpsertMyCandidateProfile;
using Aethon.Application.Candidates.Queries.GetMyCandidateProfile;
using Microsoft.AspNetCore.Mvc;

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

        group.MapPost("/profile/resumes", async (
            [FromServices] AddCandidateResumeHandler handler,
            HttpContext httpContext,
            AddCandidateResumeCommand command,
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

        group.MapPost("/profile/resumes/{resumeId:guid}/default", async (
            [FromServices] SetDefaultCandidateResumeHandler handler,
            Guid resumeId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(
                new SetDefaultCandidateResumeCommand
                {
                    ResumeId = resumeId
                },
                ct);

            return result.ToMinimalApiResult();
        });

        group.MapDelete("/profile/resumes/{resumeId:guid}", async (
            [FromServices] RemoveCandidateResumeHandler handler,
            Guid resumeId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(
                new RemoveCandidateResumeCommand
                {
                    ResumeId = resumeId
                },
                ct);

            return result.ToMinimalApiResult();
        });
    }
}