using Aethon.Api.Common;
using Aethon.Application.Organisations.Commands.AcceptOrganisationInvite;
using Aethon.Data;
using Microsoft.EntityFrameworkCore;
using Aethon.Application.Organisations.Commands.AddOrganisationDomain;
using Aethon.Application.Organisations.Commands.CancelClaimRequest;
using Aethon.Application.Organisations.Commands.ConfirmDomainVerification;
using Aethon.Application.Organisations.Commands.CreateOrganisationInvite;
using Aethon.Application.Organisations.Commands.RegenerateDomainVerificationToken;
using Aethon.Application.Organisations.Commands.SubmitOrganisationClaim;
using Aethon.Application.Organisations.Commands.UpdateMyOrganisationProfile;
using Aethon.Application.Organisations.Queries.GetClaimableOrganisations;
using Aethon.Application.Organisations.Queries.GetMyClaimRequests;
using Aethon.Application.Organisations.Queries.GetMyOrganisationProfile;
using Aethon.Application.Organisations.Queries.GetOrganisationDomains;
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

        // GET /api/v1/organisations/me/domains
        group.MapGet("/me/domains", async (
            [FromServices] GetOrganisationDomainsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.ToMinimalApiResult();
        });

        // POST /api/v1/organisations/me/domains
        group.MapPost("/me/domains", async (
            [FromServices] AddOrganisationDomainHandler handler,
            HttpContext ctx,
            AddOrganisationDomainRequestDto request,
            CancellationToken ct) =>
        {
            var validation = await ctx.ValidateAsync(request, ct);
            if (validation is not null) return validation;

            var result = await handler.HandleAsync(request, ct);
            return result.ToMinimalApiResult();
        });

        // POST /api/v1/organisations/me/domains/{id}/confirm-verification
        group.MapPost("/me/domains/{domainId:guid}/confirm-verification", async (
            [FromServices] ConfirmDomainVerificationHandler handler,
            HttpContext ctx,
            Guid domainId,
            ConfirmDomainVerificationRequestDto request,
            CancellationToken ct) =>
        {
            var validation = await ctx.ValidateAsync(request, ct);
            if (validation is not null) return validation;

            var result = await handler.HandleAsync(domainId, request, ct);
            return result.ToMinimalApiResult();
        });

        // POST /api/v1/organisations/me/domains/{id}/regenerate-token
        group.MapPost("/me/domains/{domainId:guid}/regenerate-token", async (
            [FromServices] RegenerateDomainVerificationTokenHandler handler,
            Guid domainId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(domainId, ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/organisations/check-slug?slug=...
        group.MapGet("/check-slug", async (
            AethonDbContext db,
            string slug,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(slug))
                return Results.BadRequest(new { available = false, message = "Slug is required." });

            var normalised = slug.Trim().ToLowerInvariant();
            var taken = await db.Set<Aethon.Data.Entities.Organisation>()
                .AnyAsync(o => o.Slug == normalised, ct);

            return Results.Ok(new { available = !taken, slug = normalised });
        });

        // GET /api/v1/organisations/claimable
        group.MapGet("/claimable", async (
            [FromServices] GetClaimableOrganisationsHandler handler,
            string? search,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(search, ct);
            return result.ToMinimalApiResult();
        }).AllowAnonymous();

        // POST /api/v1/organisations/claim
        group.MapPost("/claim", async (
            [FromServices] SubmitOrganisationClaimHandler handler,
            HttpContext ctx,
            SubmitClaimRequestDto request,
            CancellationToken ct) =>
        {
            var validation = await ctx.ValidateAsync(request, ct);
            if (validation is not null) return validation;

            var result = await handler.HandleAsync(request, ct);
            return result.ToMinimalApiResult();
        });

        // GET /api/v1/organisations/me/claim-requests
        group.MapGet("/me/claim-requests", async (
            [FromServices] GetMyClaimRequestsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(ct);
            return result.ToMinimalApiResult();
        });

        // DELETE /api/v1/organisations/me/claim-requests/{id}
        group.MapDelete("/me/claim-requests/{claimId:guid}", async (
            [FromServices] CancelClaimRequestHandler handler,
            Guid claimId,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(claimId, ct);
            return result.ToMinimalApiResult();
        });
    }
}
