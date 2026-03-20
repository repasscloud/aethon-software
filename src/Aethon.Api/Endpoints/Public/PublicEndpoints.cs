using Aethon.Api.Common;
using Aethon.Application.Organisations.Queries.GetPublicOrganisationProfile;
using Microsoft.AspNetCore.Mvc;

namespace Aethon.Api.Endpoints.Public;

public static class PublicEndpoints
{
    public static void MapPublicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/public")
            .AllowAnonymous()
            .WithTags("Public");

        // GET /api/v1/public/organisations/{slug}
        group.MapGet("/organisations/{slug}", async (
            [FromServices] GetPublicOrganisationProfileHandler handler,
            string slug,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(slug, ct);
            return result.ToMinimalApiResult();
        });
    }
}
