using Aethon.Application.Abstractions.Settings;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace Aethon.Api.Infrastructure.Stripe;

/// <summary>
/// Processes verified Stripe webhook events and applies the corresponding
/// business logic (verification, credit grants, etc.).
/// </summary>
public sealed class StripeWebhookProcessor
{
    private readonly AethonDbContext _db;
    private readonly IOrganisationAutoVerifier _autoVerifier;
    private readonly ISystemSettingsService _settings;
    private readonly ILogger<StripeWebhookProcessor> _logger;

    public StripeWebhookProcessor(
        AethonDbContext db,
        IOrganisationAutoVerifier autoVerifier,
        ISystemSettingsService settings,
        ILogger<StripeWebhookProcessor> logger)
    {
        _db = db;
        _autoVerifier = autoVerifier;
        _settings = settings;
        _logger = logger;
    }

    public async Task ProcessAsync(Event stripeEvent, StripePaymentEvent dbEvent, CancellationToken ct)
    {
        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
                await HandleCheckoutCompletedAsync(stripeEvent, dbEvent, ct);
                break;

            default:
                // Unhandled event type — stored as-is for manual review if needed
                _logger.LogDebug("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                break;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    private async Task HandleCheckoutCompletedAsync(Event stripeEvent, StripePaymentEvent dbEvent, CancellationToken ct)
    {
        if (stripeEvent.Data.Object is not Session session)
        {
            dbEvent.Status = StripeEventStatus.Failed;
            dbEvent.InternalNotes = "checkout.session.completed: could not cast Data.Object to Session.";
            await _db.SaveChangesAsync(ct);
            return;
        }

        var metadata = session.Metadata ?? new Dictionary<string, string>();

        // Populate db event fields from metadata
        metadata.TryGetValue("organisation_id", out var orgIdStr);
        metadata.TryGetValue("purchase_type", out var purchaseType);
        metadata.TryGetValue("price_id", out var priceId);
        metadata.TryGetValue("product_id", out var productId);

        dbEvent.PurchaseType = purchaseType;
        dbEvent.PriceId = priceId ?? session.LineItems?.Data.FirstOrDefault()?.Price?.Id;
        dbEvent.ProductId = productId ?? session.LineItems?.Data.FirstOrDefault()?.Price?.ProductId;
        dbEvent.PurchaseMetaJson = System.Text.Json.JsonSerializer.Serialize(metadata);

        if (!Guid.TryParse(orgIdStr, out var organisationId))
        {
            dbEvent.Status = StripeEventStatus.Failed;
            dbEvent.InternalNotes = "checkout.session.completed: missing or invalid organisation_id in metadata.";
            await _db.SaveChangesAsync(ct);
            return;
        }

        dbEvent.OrganisationId = organisationId;

        var org = await _db.Organisations.FirstOrDefaultAsync(o => o.Id == organisationId, ct);
        if (org is null)
        {
            dbEvent.Status = StripeEventStatus.Failed;
            dbEvent.InternalNotes = $"checkout.session.completed: organisation {organisationId} not found.";
            await _db.SaveChangesAsync(ct);
            return;
        }

        // Store the Stripe Customer ID on the org if not already set
        if (!string.IsNullOrEmpty(session.CustomerId) && string.IsNullOrEmpty(org.StripeCustomerId))
            org.StripeCustomerId = session.CustomerId;

        switch (purchaseType)
        {
            case "verification":
                await HandleVerificationAsync(org, dbEvent, metadata, ct);
                break;

            case "bundle_verification_post":
                await HandleBundleVerificationPostAsync(org, dbEvent, metadata, ct);
                break;

            case "job_credits":
                await HandleJobCreditsAsync(org, dbEvent, metadata, ct);
                break;

            case "sticky":
                await HandleStickyCreditsAsync(org, dbEvent, metadata, ct);
                break;

            case "addon":
                await HandleAddonAsync(org, dbEvent, metadata, ct);
                break;

            case "job_addons":
                await HandleJobAddonsAsync(org, dbEvent, metadata, ct);
                break;

            default:
                dbEvent.Status = StripeEventStatus.Pending;
                dbEvent.InternalNotes = $"Unknown purchase_type '{purchaseType}'. Requires manual review.";
                await _db.SaveChangesAsync(ct);
                break;
        }
    }

    // ─── Verification ────────────────────────────────────────────────────────

    private async Task HandleVerificationAsync(
        Organisation org,
        StripePaymentEvent dbEvent,
        IDictionary<string, string> metadata,
        CancellationToken ct)
    {
        metadata.TryGetValue("verification_tier", out var tierStr);

        org.VerificationPaidAt = DateTime.UtcNow;
        org.VerificationExpiresAt = DateTime.UtcNow.AddYears(1);
        org.VerificationStripeEventId = dbEvent.StripeEventId;

        if (tierStr == "enhanced")
        {
            // Enhanced always goes to manual admin review
            dbEvent.Status = StripeEventStatus.Pending;
            dbEvent.InternalNotes = "Enhanced Trusted Employer verification payment received. Requires manual admin review.";
            await _db.SaveChangesAsync(ct);
            return;
        }

        // Standard — attempt auto-verification
        bool passed = await _autoVerifier.CheckAsync(org, ct);

        if (passed)
        {
            org.VerificationTier = VerificationTier.StandardEmployer;
            org.VerifiedUtc = DateTime.UtcNow;

            await ConvertPromoCreditsToPremiumAsync(org.Id, ct);

            dbEvent.Status = StripeEventStatus.Completed;
            dbEvent.InternalNotes = "Standard Employer Verification: auto-verification passed.";
        }
        else
        {
            dbEvent.Status = StripeEventStatus.Pending;
            dbEvent.InternalNotes = "Standard Employer Verification: auto-verification did not pass. Requires manual admin review.";
        }

        await _db.SaveChangesAsync(ct);
    }

    // ─── Bundle: Verification + First Post ───────────────────────────────────

    private async Task HandleBundleVerificationPostAsync(
        Organisation org,
        StripePaymentEvent dbEvent,
        IDictionary<string, string> metadata,
        CancellationToken ct)
    {
        metadata.TryGetValue("verification_tier", out var tierStr);
        metadata.TryGetValue("credit_type", out var creditTypeStr);
        var creditQty = ParseInt(metadata, "credit_quantity", 1);

        org.VerificationPaidAt = DateTime.UtcNow;
        org.VerificationExpiresAt = DateTime.UtcNow.AddYears(1);
        org.VerificationStripeEventId = dbEvent.StripeEventId;

        // Grant the bundled posting credit
        var creditType = ParseCreditType(creditTypeStr) ?? CreditType.JobPostingStandard;
        GrantCredit(org.Id, creditType, CreditSource.StripePurchase, creditQty, null, dbEvent.Id);

        if (tierStr == "enhanced")
        {
            dbEvent.Status = StripeEventStatus.Pending;
            dbEvent.InternalNotes = $"Bundle (Enhanced + Premium post): payment received. 1x {creditType} credit granted. Verification requires manual admin review.";
            await _db.SaveChangesAsync(ct);
            return;
        }

        bool passed = await _autoVerifier.CheckAsync(org, ct);

        if (passed)
        {
            org.VerificationTier = VerificationTier.StandardEmployer;
            org.VerifiedUtc = DateTime.UtcNow;
            await ConvertPromoCreditsToPremiumAsync(org.Id, ct);
            dbEvent.Status = StripeEventStatus.Completed;
            dbEvent.InternalNotes = $"Bundle (Standard + Standard post): auto-verification passed. 1x {creditType} credit granted.";
        }
        else
        {
            dbEvent.Status = StripeEventStatus.Pending;
            dbEvent.InternalNotes = $"Bundle (Standard + Standard post): auto-verification did not pass. 1x {creditType} credit granted. Verification requires manual admin review.";
        }

        await _db.SaveChangesAsync(ct);
    }

    // ─── Job Credits ─────────────────────────────────────────────────────────

    private async Task HandleJobCreditsAsync(
        Organisation org,
        StripePaymentEvent dbEvent,
        IDictionary<string, string> metadata,
        CancellationToken ct)
    {
        metadata.TryGetValue("credit_type", out var creditTypeStr);
        var qty = ParseInt(metadata, "credit_quantity", 1);
        var creditType = ParseCreditType(creditTypeStr) ?? CreditType.JobPostingStandard;

        GrantCredit(org.Id, creditType, CreditSource.StripePurchase, qty, null, dbEvent.Id);

        dbEvent.Status = StripeEventStatus.Completed;
        dbEvent.InternalNotes = $"Granted {qty}x {creditType} credit(s).";
        await _db.SaveChangesAsync(ct);
    }

    // ─── Sticky Credits ───────────────────────────────────────────────────────

    private async Task HandleStickyCreditsAsync(
        Organisation org,
        StripePaymentEvent dbEvent,
        IDictionary<string, string> metadata,
        CancellationToken ct)
    {
        metadata.TryGetValue("credit_type", out var creditTypeStr);
        var qty = ParseInt(metadata, "credit_quantity", 1);
        var creditType = ParseCreditType(creditTypeStr) ?? CreditType.StickyTop24h;

        GrantCredit(org.Id, creditType, CreditSource.StripePurchase, qty, null, dbEvent.Id);

        dbEvent.Status = StripeEventStatus.Completed;
        dbEvent.InternalNotes = $"Granted {qty}x {creditType} sticky credit(s).";
        await _db.SaveChangesAsync(ct);
    }

    // ─── Add-on (charged on existing published job) ───────────────────────────

    private async Task HandleAddonAsync(
        Organisation org,
        StripePaymentEvent dbEvent,
        IDictionary<string, string> metadata,
        CancellationToken ct)
    {
        // The job ID is in metadata so the billing endpoint can apply the add-on
        // after payment confirmation. This handler records the completion so the
        // polling/callback mechanism can unlock the job update.
        metadata.TryGetValue("job_id", out var jobIdStr);
        metadata.TryGetValue("addon_type", out var addonType);

        if (!Guid.TryParse(jobIdStr, out var jobId))
        {
            dbEvent.Status = StripeEventStatus.Failed;
            dbEvent.InternalNotes = $"addon purchase: missing or invalid job_id in metadata.";
            await _db.SaveChangesAsync(ct);
            return;
        }

        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job is null)
        {
            dbEvent.Status = StripeEventStatus.Failed;
            dbEvent.InternalNotes = $"addon purchase: job {jobId} not found.";
            await _db.SaveChangesAsync(ct);
            return;
        }

        switch (addonType)
        {
            case "video":
                metadata.TryGetValue("video_youtube_id", out var ytId);
                metadata.TryGetValue("video_vimeo_id", out var vimeoId);
                job.VideoYouTubeId = !string.IsNullOrEmpty(ytId) ? ytId : job.VideoYouTubeId;
                job.VideoVimeoId = !string.IsNullOrEmpty(vimeoId) ? vimeoId : job.VideoVimeoId;
                break;

            case "highlight":
                metadata.TryGetValue("highlight_colour", out var colour);
                job.IsHighlighted = true;
                job.HighlightColour = colour;
                break;

            case "ai_matching":
                job.HasAiCandidateMatching = true;
                break;
        }

        job.UpdatedUtc = DateTime.UtcNow;
        dbEvent.Status = StripeEventStatus.Completed;
        dbEvent.InternalNotes = $"Add-on '{addonType}' applied to job {jobId}.";
        await _db.SaveChangesAsync(ct);
    }

    // ─── Job Add-ons + Publish (new Stripe Checkout flow) ────────────────────

    private async Task HandleJobAddonsAsync(
        Organisation org,
        StripePaymentEvent dbEvent,
        IDictionary<string, string> metadata,
        CancellationToken ct)
    {
        metadata.TryGetValue("job_id", out var jobIdStr);

        if (!Guid.TryParse(jobIdStr, out var jobId))
        {
            dbEvent.Status = StripeEventStatus.Failed;
            dbEvent.InternalNotes = "job_addons: missing or invalid job_id in metadata.";
            await _db.SaveChangesAsync(ct);
            return;
        }

        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job is null)
        {
            dbEvent.Status = StripeEventStatus.Failed;
            dbEvent.InternalNotes = $"job_addons: job {jobId} not found.";
            await _db.SaveChangesAsync(ct);
            return;
        }

        var now = DateTime.UtcNow;

        // Apply add-ons from metadata flags
        if (metadata.TryGetValue("add_highlight", out var addHighlight) && addHighlight == "true")
        {
            job.IsHighlighted = true;
            if (metadata.TryGetValue("highlight_colour", out var colour) && !string.IsNullOrEmpty(colour))
                job.HighlightColour = colour;
        }

        if (metadata.TryGetValue("add_video", out var addVideo) && addVideo == "true")
        {
            metadata.TryGetValue("video_youtube_id", out var ytId);
            metadata.TryGetValue("video_vimeo_id", out var vimeoId);
            if (!string.IsNullOrEmpty(ytId))  job.VideoYouTubeId = ytId;
            if (!string.IsNullOrEmpty(vimeoId)) job.VideoVimeoId = vimeoId;
        }

        if (metadata.TryGetValue("add_ai_matching", out var addAi) && addAi == "true")
            job.HasAiCandidateMatching = true;

        if (metadata.TryGetValue("sticky_duration", out var stickyStr) && int.TryParse(stickyStr, out var stickyDays) && stickyDays > 0)
            job.StickyUntilUtc = now.AddDays(stickyDays);

        // Publish the job
        job.Status = JobStatus.Published;
        job.PublishedUtc ??= now;
        job.UpdatedUtc = now;

        dbEvent.Status = StripeEventStatus.Completed;
        dbEvent.InternalNotes = $"job_addons: add-ons applied and job {jobId} published.";
        await _db.SaveChangesAsync(ct);
    }

    // ─── Credit conversion (Standard promo → Premium on verification) ─────────

    private async Task ConvertPromoCreditsToPremiumAsync(Guid organisationId, CancellationToken ct)
    {
        // Respect the admin toggle — default ON if the setting doesn't exist yet
        var shouldConvert = await _settings.GetBoolAsync(
            SystemSettingKeys.FeatureVerificationUpgradesPromoCredits, defaultValue: true, ct: ct);
        if (!shouldConvert) return;

        var now = DateTime.UtcNow;

        var promoCredits = await _db.OrganisationJobCredits
            .Where(c =>
                c.OrganisationId == organisationId &&
                c.CreditType == CreditType.JobPostingStandard &&
                c.Source == CreditSource.LaunchPromotion &&
                c.QuantityRemaining > 0 &&
                c.ConvertedAt == null &&
                (c.ExpiresAt == null || c.ExpiresAt > now))
            .ToListAsync(ct);

        foreach (var credit in promoCredits)
        {
            credit.CreditType = CreditType.JobPostingPremium;
            credit.ConvertedAt = now;
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private void GrantCredit(
        Guid organisationId,
        CreditType type,
        CreditSource source,
        int quantity,
        DateTime? expiresAt,
        Guid stripePaymentEventId)
    {
        _db.OrganisationJobCredits.Add(new OrganisationJobCredit
        {
            Id = Guid.NewGuid(),
            OrganisationId = organisationId,
            CreditType = type,
            Source = source,
            QuantityOriginal = quantity,
            QuantityRemaining = quantity,
            ExpiresAt = expiresAt,
            StripePaymentEventId = stripePaymentEventId,
            CreatedUtc = DateTime.UtcNow
        });
    }

    private static int ParseInt(IDictionary<string, string> meta, string key, int fallback)
        => meta.TryGetValue(key, out var v) && int.TryParse(v, out var i) ? i : fallback;

    private static CreditType? ParseCreditType(string? value) => value switch
    {
        "standard"      => CreditType.JobPostingStandard,
        "premium"       => CreditType.JobPostingPremium,
        "sticky_24h"    => CreditType.StickyTop24h,
        "sticky_7d"     => CreditType.StickyTop7d,
        "sticky_30d"    => CreditType.StickyTop30d,
        _               => null
    };
}
