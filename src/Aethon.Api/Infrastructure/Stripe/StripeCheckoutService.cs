using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Settings;
using Aethon.Data;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace Aethon.Api.Infrastructure.Stripe;

public sealed class StripeCheckoutService
{
    // Maps SystemSettingKeys price key → (credit type, quantity to grant)
    private static readonly Dictionary<string, (CreditType Type, int Qty, string PurchaseType)> CreditPriceMap = new()
    {
        [SystemSettingKeys.StripePriceJobStandard1x]         = (CreditType.JobPostingStandard, 1,  "job_credits"),
        [SystemSettingKeys.StripePriceJobStandard5x]         = (CreditType.JobPostingStandard, 5,  "job_credits"),
        [SystemSettingKeys.StripePriceJobStandard10x]        = (CreditType.JobPostingStandard, 10, "job_credits"),
        [SystemSettingKeys.StripePriceJobStandard20x]        = (CreditType.JobPostingStandard, 20, "job_credits"),
        [SystemSettingKeys.StripePriceJobPremium1x]          = (CreditType.JobPostingPremium,  1,  "job_credits"),
        [SystemSettingKeys.StripePriceJobPremium5x]          = (CreditType.JobPostingPremium,  5,  "job_credits"),
        [SystemSettingKeys.StripePriceJobPremium10x]         = (CreditType.JobPostingPremium,  10, "job_credits"),
        [SystemSettingKeys.StripePriceJobPremium20x]         = (CreditType.JobPostingPremium,  20, "job_credits"),
        [SystemSettingKeys.StripePriceStickyVerified24h]     = (CreditType.StickyTop24h,       1,  "sticky"),
        [SystemSettingKeys.StripePriceStickyVerified7d]      = (CreditType.StickyTop7d,        1,  "sticky"),
        [SystemSettingKeys.StripePriceStickyVerified30d]     = (CreditType.StickyTop30d,       1,  "sticky"),
        [SystemSettingKeys.StripePriceStickyUnverified24h]   = (CreditType.StickyTop24h,       1,  "sticky"),
        [SystemSettingKeys.StripePriceStickyUnverified7d]    = (CreditType.StickyTop7d,        1,  "sticky"),
        [SystemSettingKeys.StripePriceStickyUnverified30d]   = (CreditType.StickyTop30d,       1,  "sticky"),
    };

    private static string CreditTypeToMetadata(CreditType t) => t switch
    {
        CreditType.JobPostingStandard => "standard",
        CreditType.JobPostingPremium  => "premium",
        CreditType.StickyTop24h       => "sticky_24h",
        CreditType.StickyTop7d        => "sticky_7d",
        CreditType.StickyTop30d       => "sticky_30d",
        _                             => "unknown"
    };

    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly ISystemSettingsService _settings;
    private readonly IConfiguration _config;

    public StripeCheckoutService(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        ISystemSettingsService settings,
        IConfiguration config)
    {
        _db = db;
        _currentUser = currentUser;
        _settings = settings;
        _config = config;
    }

    // ─── Verification checkout ────────────────────────────────────────────────

    public async Task<(string? Url, string? Error)> CreateVerificationCheckoutAsync(
        string tier, CancellationToken ct)
    {
        var (org, error) = await LoadOrgAsync(ct);
        if (org is null) return (null, error);

        var priceKey = tier == "enhanced"
            ? SystemSettingKeys.StripePriceVerificationEnhanced
            : SystemSettingKeys.StripePriceVerificationStandard;

        var priceId = await _settings.GetStringAsync(priceKey, ct);
        if (string.IsNullOrEmpty(priceId))
            return (null, "Verification product not yet configured. Please contact support.");

        var customerId = await EnsureStripeCustomerAsync(org, ct);
        var webBase = _config["Email:WebBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5200";

        var bundlePriceKey = tier == "enhanced"
            ? SystemSettingKeys.StripePriceBundleEnhancedVerificationPost
            : SystemSettingKeys.StripePriceBundleStandardVerificationPost;

        var sessionOptions = new SessionCreateOptions
        {
            Customer = customerId,
            Mode = "payment",
            LineItems = [new() { Price = priceId, Quantity = 1 }],
            Metadata = new Dictionary<string, string>
            {
                ["organisation_id"]    = org.Id.ToString(),
                ["purchase_type"]      = "verification",
                ["verification_tier"]  = tier
            },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                SetupFutureUsage = "off_session"
            },
            AllowPromotionCodes = true,
            SuccessUrl = $"{webBase}/app/verification/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl  = $"{webBase}/app/verification/cancelled"
        };

        var service = new SessionService();
        var session = await service.CreateAsync(sessionOptions, cancellationToken: ct);
        return (session.Url, null);
    }

    // ─── Bundle checkout (verification + first post) ─────────────────────────

    public async Task<(string? Url, string? Error)> CreateBundleCheckoutAsync(
        string tier, CancellationToken ct)
    {
        var (org, error) = await LoadOrgAsync(ct);
        if (org is null) return (null, error);

        var priceKey = tier == "enhanced"
            ? SystemSettingKeys.StripePriceBundleEnhancedVerificationPost
            : SystemSettingKeys.StripePriceBundleStandardVerificationPost;

        var priceId = await _settings.GetStringAsync(priceKey, ct);
        if (string.IsNullOrEmpty(priceId))
            return (null, "Bundle product not yet configured. Please contact support.");

        var creditType = tier == "enhanced"
            ? CreditType.JobPostingPremium
            : CreditType.JobPostingStandard;

        var customerId = await EnsureStripeCustomerAsync(org, ct);
        var webBase = _config["Email:WebBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5200";

        var sessionOptions = new SessionCreateOptions
        {
            Customer = customerId,
            Mode = "payment",
            LineItems = [new() { Price = priceId, Quantity = 1 }],
            Metadata = new Dictionary<string, string>
            {
                ["organisation_id"]    = org.Id.ToString(),
                ["purchase_type"]      = "bundle_verification_post",
                ["verification_tier"]  = tier,
                ["credit_type"]        = CreditTypeToMetadata(creditType),
                ["credit_quantity"]    = "1"
            },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                SetupFutureUsage = "off_session"
            },
            AllowPromotionCodes = true,
            SuccessUrl = $"{webBase}/app/verification/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl  = $"{webBase}/app/verification/cancelled"
        };

        var service = new SessionService();
        var session = await service.CreateAsync(sessionOptions, cancellationToken: ct);
        return (session.Url, null);
    }

    // ─── Job credits / sticky checkout ───────────────────────────────────────

    public async Task<(string? Url, string? Error)> CreateCreditsCheckoutAsync(
        string priceKey, CancellationToken ct)
    {
        if (!CreditPriceMap.TryGetValue(priceKey, out var creditInfo))
            return (null, "Invalid price key.");

        var (org, error) = await LoadOrgAsync(ct);
        if (org is null) return (null, error);

        var priceId = await _settings.GetStringAsync(priceKey, ct);
        if (string.IsNullOrEmpty(priceId))
            return (null, "Product not yet configured. Please contact support.");

        var customerId = await EnsureStripeCustomerAsync(org, ct);
        var webBase = _config["Email:WebBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5200";

        var sessionOptions = new SessionCreateOptions
        {
            Customer = customerId,
            Mode = "payment",
            LineItems = [new() { Price = priceId, Quantity = 1 }],
            Metadata = new Dictionary<string, string>
            {
                ["organisation_id"] = org.Id.ToString(),
                ["purchase_type"]   = creditInfo.PurchaseType,
                ["price_key"]       = priceKey,
                ["credit_type"]     = CreditTypeToMetadata(creditInfo.Type),
                ["credit_quantity"] = creditInfo.Qty.ToString()
            },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                SetupFutureUsage = "off_session"
            },
            AllowPromotionCodes = true,
            SuccessUrl = $"{webBase}/app/organisation/billing?payment=success",
            CancelUrl  = $"{webBase}/app/organisation/billing"
        };

        var service = new SessionService();
        var session = await service.CreateAsync(sessionOptions, cancellationToken: ct);
        return (session.Url, null);
    }

    // ─── Billing portal ───────────────────────────────────────────────────────

    public async Task<(string? Url, string? Error)> CreateBillingPortalUrlAsync(CancellationToken ct)
    {
        var (org, error) = await LoadOrgAsync(ct);
        if (org is null) return (null, error);

        if (string.IsNullOrEmpty(org.StripeCustomerId))
            return (null, "No billing account found. Complete a purchase first.");

        var webBase = _config["Email:WebBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5200";

        var portalOptions = new global::Stripe.BillingPortal.SessionCreateOptions
        {
            Customer  = org.StripeCustomerId,
            ReturnUrl = $"{webBase}/app/organisation/billing"
        };

        var service = new global::Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(portalOptions, cancellationToken: ct);
        return (session.Url, null);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task<(Aethon.Data.Entities.Organisation? Org, string? Error)> LoadOrgAsync(CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return (null, "Not authenticated.");

        var membership = await _db.OrganisationMemberships
            .Include(m => m.Organisation)
            .Where(m => m.UserId == _currentUser.UserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (membership is null)
            return (null, "No active organisation membership found.");

        return (membership.Organisation, null);
    }

    private async Task<string?> EnsureStripeCustomerAsync(
        Aethon.Data.Entities.Organisation org, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(org.StripeCustomerId))
            return org.StripeCustomerId;

        var customerService = new CustomerService();
        var customer = await customerService.CreateAsync(new CustomerCreateOptions
        {
            Name     = org.Name,
            Metadata = new Dictionary<string, string> { ["organisation_id"] = org.Id.ToString() }
        }, cancellationToken: ct);

        org.StripeCustomerId = customer.Id;
        org.UpdatedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return customer.Id;
    }
}
