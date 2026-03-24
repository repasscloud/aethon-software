using Aethon.Api.Common;
using Aethon.Api.Infrastructure.Stripe;
using Aethon.Data;
using Aethon.Shared.Billing;
using Aethon.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aethon.Application.Abstractions.Authentication;

namespace Aethon.Api.Endpoints.Billing;

public static class BillingEndpoints
{
    public static void MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/billing")
            .RequireAuthorization()
            .WithTags("Billing");

        // POST /api/v1/billing/verification/checkout
        // Creates a Stripe Checkout session for Standard or Enhanced verification.
        group.MapPost("/verification/checkout", async (
            [FromServices] StripeCheckoutService checkout,
            VerificationCheckoutRequestDto request,
            CancellationToken ct) =>
        {
            if (request.Tier is not ("standard" or "enhanced"))
                return Results.BadRequest(new { code = "billing.invalid_tier", message = "Tier must be 'standard' or 'enhanced'." });

            var (url, error) = await checkout.CreateVerificationCheckoutAsync(request.Tier, ct);
            if (url is null)
                return Results.BadRequest(new { code = "billing.checkout_failed", message = error });

            return Results.Ok(new BillingCheckoutResponseDto { CheckoutUrl = url });
        });

        // POST /api/v1/billing/verification/bundle-checkout
        // Creates a Stripe Checkout session for the verification + first post bundle.
        group.MapPost("/verification/bundle-checkout", async (
            [FromServices] StripeCheckoutService checkout,
            VerificationCheckoutRequestDto request,
            CancellationToken ct) =>
        {
            if (request.Tier is not ("standard" or "enhanced"))
                return Results.BadRequest(new { code = "billing.invalid_tier", message = "Tier must be 'standard' or 'enhanced'." });

            var (url, error) = await checkout.CreateBundleCheckoutAsync(request.Tier, ct);
            if (url is null)
                return Results.BadRequest(new { code = "billing.checkout_failed", message = error });

            return Results.Ok(new BillingCheckoutResponseDto { CheckoutUrl = url });
        });

        // POST /api/v1/billing/credits/checkout
        // Creates a Stripe Checkout session for job posting credits or sticky credits.
        group.MapPost("/credits/checkout", async (
            [FromServices] StripeCheckoutService checkout,
            CreditsCheckoutRequestDto request,
            CancellationToken ct) =>
        {
            var (url, error) = await checkout.CreateCreditsCheckoutAsync(request.PriceKey, ct);
            if (url is null)
                return Results.BadRequest(new { code = "billing.checkout_failed", message = error });

            return Results.Ok(new BillingCheckoutResponseDto { CheckoutUrl = url });
        });

        // GET /api/v1/billing/portal
        // Creates a Stripe Billing Portal session and returns the URL.
        group.MapGet("/portal", async (
            [FromServices] StripeCheckoutService checkout,
            CancellationToken ct) =>
        {
            var (url, error) = await checkout.CreateBillingPortalUrlAsync(ct);
            if (url is null)
                return Results.BadRequest(new { code = "billing.portal_failed", message = error });

            return Results.Ok(new { url });
        });

        // GET /api/v1/billing/me/credits
        // Returns all job credit rows for the current user's organisation.
        group.MapGet("/me/credits", async (
            [FromServices] ICurrentUserAccessor currentUser,
            AethonDbContext db,
            CancellationToken ct) =>
        {
            if (!currentUser.IsAuthenticated)
                return Results.Unauthorized();

            var membership = await db.OrganisationMemberships
                .AsNoTracking()
                .Where(m => m.UserId == currentUser.UserId && m.Status == MembershipStatus.Active)
                .OrderByDescending(m => m.IsOwner)
                .FirstOrDefaultAsync(ct);

            if (membership is null)
                return Results.NotFound(new { code = "organisations.not_found" });

            var credits = await db.OrganisationJobCredits
                .AsNoTracking()
                .Where(c => c.OrganisationId == membership.OrganisationId)
                .OrderByDescending(c => c.CreatedUtc)
                .Select(c => new CreditBalanceItemDto
                {
                    Id               = c.Id,
                    CreditType       = c.CreditType,
                    Source           = c.Source,
                    QuantityOriginal  = c.QuantityOriginal,
                    QuantityRemaining = c.QuantityRemaining,
                    ExpiresAt        = c.ExpiresAt,
                    ConvertedAt      = c.ConvertedAt,
                    CreatedUtc       = c.CreatedUtc
                })
                .ToListAsync(ct);

            return Results.Ok(credits);
        });
    }
}
