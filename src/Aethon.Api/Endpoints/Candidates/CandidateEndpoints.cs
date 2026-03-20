using Aethon.Api.Common;
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
    }
}
