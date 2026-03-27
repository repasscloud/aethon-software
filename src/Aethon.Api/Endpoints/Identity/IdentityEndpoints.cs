using Aethon.Api.Common;
using Aethon.Application.Verification.Commands.SubmitVerificationRequest;
using Aethon.Application.Verification.Queries.GetMyVerificationRequest;
using Aethon.Shared.Verification;
using Microsoft.AspNetCore.Mvc;

namespace Aethon.Api.Endpoints.Identity;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/identity")
            .RequireAuthorization()
            .WithTags("Identity");

        // POST /api/v1/identity/verification-request
        // Submit an identity verification request.
        // Gates: email confirmed, not already verified, no pending request.
        group.MapPost("/verification-request", async (
            [FromServices] SubmitVerificationRequestHandler handler,
            SubmitVerificationRequestDto request,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/identity/verification-request/mine
        // Returns the caller's most recent verification request, or null if none.
        group.MapGet("/verification-request/mine", async (
            [FromServices] GetMyVerificationRequestHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.ToMinimalApiResult();
        });
    }
}
