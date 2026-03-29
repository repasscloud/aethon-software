using Aethon.Api.Common;
using Aethon.Api.Infrastructure.Stripe;
using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Settings;
using Aethon.Data;
using Aethon.Shared.Billing;
using Aethon.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        // POST /api/v1/billing/job-publish-checkout
        // Consumes posting credit (if available), adds chargeable add-on line items, and
        // either publishes the job immediately or creates a Stripe Checkout Session.
        group.MapPost("/job-publish-checkout", async (
            [FromServices] StripeCheckoutService checkout,
            [FromServices] ILoggerFactory loggerFactory,
            JobPublishCheckoutRequestDto request,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("BillingEndpoints");
            try
            {
                var (published, checkoutUrl, error) = await checkout.CreateJobPublishCheckoutAsync(request, ct);

                if (error is not null)
                    return Results.BadRequest(new { code = "billing.checkout_failed", message = error });

                return Results.Ok(new JobPublishCheckoutResponseDto
                {
                    Published   = published,
                    CheckoutUrl = checkoutUrl
                });
            }
            catch (Stripe.StripeException ex)
            {
                logger.LogError(ex, "Stripe error creating job publish checkout for job {JobId}: {StripeError}", request.JobId, ex.StripeError?.Message ?? ex.Message);
                return Results.BadRequest(new
                {
                    code    = "billing.stripe_error",
                    message = $"Stripe error: {ex.StripeError?.Message ?? ex.Message}"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error creating job publish checkout for job {JobId}", request.JobId);
                return Results.BadRequest(new
                {
                    code    = "billing.checkout_error",
                    message = $"Checkout error: {ex.Message}"
                });
            }
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

        // GET /api/v1/billing/display-prices
        // Returns all Display.Price.* settings as a key→value dictionary.
        // Anonymous — prices are public information shown on pricing/billing pages.
        group.MapGet("/display-prices", async (
            ISystemSettingsService settings,
            AethonDbContext db,
            CancellationToken ct) =>
        {
            var keys = new[]
            {
                SystemSettingKeys.DisplayPriceJobStandard1x,
                SystemSettingKeys.DisplayPriceJobStandard5x,
                SystemSettingKeys.DisplayPriceJobStandard10x,
                SystemSettingKeys.DisplayPriceJobStandard20x,
                SystemSettingKeys.DisplayPriceJobPremium1x,
                SystemSettingKeys.DisplayPriceJobPremium5x,
                SystemSettingKeys.DisplayPriceJobPremium10x,
                SystemSettingKeys.DisplayPriceJobPremium20x,
                SystemSettingKeys.DisplayPriceVerificationStandard,
                SystemSettingKeys.DisplayPriceVerificationEnhanced,
                SystemSettingKeys.DisplayPriceBundleStandardVerificationPost,
                SystemSettingKeys.DisplayPriceBundleEnhancedVerificationPost,
                SystemSettingKeys.DisplayPriceAddonHighlight,
                SystemSettingKeys.DisplayPriceAddonVideo,
                SystemSettingKeys.DisplayPriceAddonAiMatching,
                SystemSettingKeys.DisplayPriceStickyVerified24h,
                SystemSettingKeys.DisplayPriceStickyVerified7d,
                SystemSettingKeys.DisplayPriceStickyVerified30d,
                SystemSettingKeys.DisplayPriceStickyUnverified24h,
                SystemSettingKeys.DisplayPriceStickyUnverified7d,
                SystemSettingKeys.DisplayPriceStickyUnverified30d,
            };

            var rows = await db.SystemSettings
                .AsNoTracking()
                .Where(s => keys.Contains(s.Key))
                .Select(s => new { s.Key, s.Value })
                .ToListAsync(ct);

            var result = rows.ToDictionary(r => r.Key, r => r.Value ?? "");
            return Results.Ok(result);
        }).AllowAnonymous();
    }
}
