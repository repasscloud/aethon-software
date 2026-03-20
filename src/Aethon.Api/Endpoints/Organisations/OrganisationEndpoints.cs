using Aethon.Api.Common;
using Aethon.Application.Organisations.Commands.AcceptOrganisationInvite;
using Aethon.Application.Organisations.Commands.CreateOrganisationInvite;
using Aethon.Application.Organisations.Commands.UpdateMyOrganisationProfile;
using Aethon.Application.Organisations.Queries.GetMyOrganisationProfile;
using Aethon.Application.Organisations.Queries.GetOrganisationMembers;
using Aethon.Shared.Organisations;
using Microsoft.AspNetCore.Mvc;

namespace Aethon.Api.Endpoints.Organisations;

public static class OrganisationEndpoints
{
    public static void MapOrganisationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/organisations")
            .RequireAuthorization()
            .WithTags("Organisations");

        // GET /api/v1/organisations/me/profile
        group.MapGet("/me/profile", async (
            [FromServices] GetMyOrganisationProfileHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.ToMinimalApiResult();
        });

        // PUT /api/v1/organisations/me/profile
        group.MapPut("/me/profile", async (
            [FromServices] UpdateMyOrganisationProfileHandler handler,
            HttpContext ctx,
            UpdateOrganisationProfileRequestDto request,
            CancellationToken ct) =>
        {
            var validation = await ctx.ValidateAsync(request, ct);
            if (validation is not null) return validation;

            var result = await handler.HandleAsync(request, ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/organisations/me/members
        group.MapGet("/me/members", async (
            [FromServices] GetOrganisationMembersHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.ToMinimalApiResult();
        });

        // POST /api/v1/organisations/me/invites
        group.MapPost("/me/invites", async (
            [FromServices] CreateOrganisationInviteHandler handler,
            HttpContext ctx,
            CreateOrganisationInviteRequestDto request,
            CancellationToken ct) =>
        {
            var validation = await ctx.ValidateAsync(request, ct);
            if (validation is not null) return validation;

            var result = await handler.HandleAsync(request, ct);
            return result.ToMinimalApiResult();
        });

        // POST /api/v1/organisations/invites/accept
        group.MapPost("/invites/accept", async (
            [FromServices] AcceptOrganisationInviteHandler handler,
            HttpContext ctx,
            AcceptOrganisationInviteRequestDto request,
            CancellationToken ct) =>
        {
            var validation = await ctx.ValidateAsync(request, ct);
            if (validation is not null) return validation;

            var result = await handler.HandleAsync(request, ct);
            return result.ToMinimalApiResult();
        });
    }
}
