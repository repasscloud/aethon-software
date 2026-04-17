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

    // ─── API key helper ───────────────────────────────────────────────────────

    /// <summary>
    /// Builds an <see cref="IStripeClient"/> keyed with the secret from the DB
    /// (admin-configurable via Admin → Stripe Products), falling back to the value
    /// from appsettings / environment variables when the DB row is empty.
    /// Passing the client to each service constructor avoids the global
    /// <see cref="StripeConfiguration.ApiKey"/> requirement.
    /// </summary>
    private async Task<IStripeClient> GetStripeClientAsync(CancellationToken ct)
    {
        var dbKey = await _settings.GetStringAsync(SystemSettingKeys.StripeSecretKey, ct);
        var key   = !string.IsNullOrWhiteSpace(dbKey) ? dbKey : StripeConfiguration.ApiKey;
        if (string.IsNullOrWhiteSpace(key))
            throw new StripeException("Stripe API key is not configured. Set it in Admin → Stripe Products → Secret API Key.");
        return new StripeClient(key);
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

        var stripeClient = await GetStripeClientAsync(ct);
        var customerId = await EnsureStripeCustomerAsync(org, stripeClient, ct);
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

        var service = new SessionService(stripeClient);
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

        var stripeClient = await GetStripeClientAsync(ct);
        var customerId = await EnsureStripeCustomerAsync(org, stripeClient, ct);
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

        var service = new SessionService(stripeClient);
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

        var stripeClient = await GetStripeClientAsync(ct);
        var customerId = await EnsureStripeCustomerAsync(org, stripeClient, ct);
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

        var service = new SessionService(stripeClient);
        var session = await service.CreateAsync(sessionOptions, cancellationToken: ct);
        return (session.Url, null);
    }

    // ─── Job publish checkout ─────────────────────────────────────────────────
    // Single endpoint that handles:
    //   • Consuming an existing posting credit (or adding it as a line item)
    //   • Add-on line items (highlight, video, AI matching) for Standard jobs
    //   • Sticky top line items or credit consumption
    // If no Stripe charge is needed the job is published immediately and
    // (Published=true, Url=null) is returned. Otherwise the job is set to OnHold,
    // a Checkout Session is created, and the URL is returned to redirect the user.

    public async Task<(bool Published, string? CheckoutUrl, string? Error)> CreateJobPublishCheckoutAsync(
        Aethon.Shared.Billing.JobPublishCheckoutRequestDto request,
        CancellationToken ct)
    {
        var (org, orgError) = await LoadOrgAsync(ct);
        if (org is null) return (false, null, orgError);

        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == request.JobId, ct);
        if (job is null) return (false, null, "Job not found.");
        if (job.OwnedByOrganisationId != org.Id)
            return (false, null, "You do not have permission to publish this job.");
        if (job.Status is not (Aethon.Shared.Enums.JobStatus.Draft
                           or Aethon.Shared.Enums.JobStatus.Approved
                           or Aethon.Shared.Enums.JobStatus.OnHold))
            return (false, null, $"Cannot publish a job with status '{job.Status}'.");

        var isPremium  = job.PostingTier == Aethon.Shared.Enums.JobPostingTier.Premium;
        var isVerified = org.VerificationTier != Aethon.Shared.Enums.VerificationTier.None;
        var now        = DateTime.UtcNow;

        var lineItems = new List<SessionLineItemOptions>();
        var meta      = new Dictionary<string, string>
        {
            ["organisation_id"] = org.Id.ToString(),
            ["purchase_type"]   = "job_addons",
            ["job_id"]          = job.Id.ToString()
        };

        if (!string.IsNullOrWhiteSpace(job.PoNumber))
            meta["po_number"] = job.PoNumber;

        // ── Base posting credit ───────────────────────────────────────────────
        var creditType = isPremium
            ? Aethon.Shared.Enums.CreditType.JobPostingPremium
            : Aethon.Shared.Enums.CreditType.JobPostingStandard;

        var postingCredit = await _db.OrganisationJobCredits
            .Where(c =>
                c.OrganisationId == org.Id &&
                c.CreditType == creditType &&
                c.QuantityRemaining > 0 &&
                (c.ExpiresAt == null || c.ExpiresAt > now))
            .OrderBy(c => c.ExpiresAt)
            .FirstOrDefaultAsync(ct);

        if (postingCredit is not null)
        {
            // Consume the credit immediately; if the user abandons checkout the credit
            // is spent (acceptable MVP trade-off — no reservation mechanism needed).
            postingCredit.QuantityRemaining--;
            _db.CreditConsumptionLogs.Add(new Aethon.Data.Entities.CreditConsumptionLog
            {
                Id                      = Guid.NewGuid(),
                OrganisationJobCreditId = postingCredit.Id,
                OrganisationId          = org.Id,
                JobId                   = job.Id,
                ConsumedByUserId        = _currentUser.UserId,
                QuantityConsumed        = 1,
                ConsumedAt              = now,
                CreatedUtc              = now,
                CreatedByUserId         = _currentUser.UserId
            });
            await _db.SaveChangesAsync(ct);
            meta["posting_credit_consumed"] = "true";
            meta["posting_credit_id"]       = postingCredit.Id.ToString();
        }
        else
        {
            // No credit — add the 1× posting price as a Stripe line item
            var priceKey = isPremium
                ? SystemSettingKeys.StripePriceJobPremium1x
                : SystemSettingKeys.StripePriceJobStandard1x;
            var priceId = await _settings.GetStringAsync(priceKey, ct);
            if (string.IsNullOrEmpty(priceId))
                return (false, null, "Job posting product not yet configured. Please contact support.");
            lineItems.Add(new SessionLineItemOptions { Price = priceId, Quantity = 1 });
            meta["posting_credit_consumed"] = "false";
        }

        // ── Standard add-ons ─────────────────────────────────────────────────
        if (!isPremium)
        {
            if (request.AddHighlight && !string.IsNullOrWhiteSpace(request.HighlightColour))
            {
                var pid = await _settings.GetStringAsync(SystemSettingKeys.StripePriceAddonHighlight, ct);
                if (string.IsNullOrEmpty(pid))
                    return (false, null, "Highlight colour product not yet configured. Please contact support.");
                lineItems.Add(new SessionLineItemOptions { Price = pid, Quantity = 1 });
                meta["add_highlight"]    = "true";
                meta["highlight_colour"] = request.HighlightColour.Trim();
            }

            if (request.AddVideo &&
                (!string.IsNullOrWhiteSpace(request.VideoYouTubeId) ||
                 !string.IsNullOrWhiteSpace(request.VideoVimeoId)))
            {
                var pid = await _settings.GetStringAsync(SystemSettingKeys.StripePriceAddonVideo, ct);
                if (string.IsNullOrEmpty(pid))
                    return (false, null, "Video embed product not yet configured. Please contact support.");
                lineItems.Add(new SessionLineItemOptions { Price = pid, Quantity = 1 });
                meta["add_video"]          = "true";
                meta["video_youtube_id"]   = request.VideoYouTubeId ?? "";
                meta["video_vimeo_id"]     = request.VideoVimeoId ?? "";
            }

            if (request.AddAiMatching)
            {
                var pid = await _settings.GetStringAsync(SystemSettingKeys.StripePriceAddonAiMatching, ct);
                if (string.IsNullOrEmpty(pid))
                    return (false, null, "AI matching product not yet configured. Please contact support.");
                lineItems.Add(new SessionLineItemOptions { Price = pid, Quantity = 1 });
                meta["add_ai_matching"] = "true";
            }
        }
        else
        {
            // Premium always includes all add-ons — apply directly, no charge
            if (request.AddHighlight && !string.IsNullOrWhiteSpace(request.HighlightColour))
            {
                meta["add_highlight"]    = "true";
                meta["highlight_colour"] = request.HighlightColour.Trim();
            }
            if (request.AddVideo)
            {
                meta["add_video"]        = "true";
                meta["video_youtube_id"] = request.VideoYouTubeId ?? "";
                meta["video_vimeo_id"]   = request.VideoVimeoId ?? "";
            }
            if (request.AddAiMatching)
                meta["add_ai_matching"] = "true";
        }

        // ── Sticky ───────────────────────────────────────────────────────────
        if (request.StickyDuration > 0)
        {
            meta["sticky_duration"] = request.StickyDuration.ToString();
            var stickyType = request.StickyDuration switch
            {
                1  => Aethon.Shared.Enums.CreditType.StickyTop24h,
                7  => Aethon.Shared.Enums.CreditType.StickyTop7d,
                _  => Aethon.Shared.Enums.CreditType.StickyTop30d
            };

            var stickyCredit = await _db.OrganisationJobCredits
                .Where(c =>
                    c.OrganisationId == org.Id &&
                    c.CreditType == stickyType &&
                    c.QuantityRemaining > 0 &&
                    (c.ExpiresAt == null || c.ExpiresAt > now))
                .OrderBy(c => c.ExpiresAt)
                .FirstOrDefaultAsync(ct);

            if (stickyCredit is not null)
            {
                stickyCredit.QuantityRemaining--;
                _db.CreditConsumptionLogs.Add(new Aethon.Data.Entities.CreditConsumptionLog
                {
                    Id                      = Guid.NewGuid(),
                    OrganisationJobCreditId = stickyCredit.Id,
                    OrganisationId          = org.Id,
                    JobId                   = job.Id,
                    ConsumedByUserId        = _currentUser.UserId,
                    QuantityConsumed        = 1,
                    ConsumedAt              = now,
                    CreatedUtc              = now,
                    CreatedByUserId         = _currentUser.UserId
                });
                await _db.SaveChangesAsync(ct);
                meta["sticky_consumed_credit"] = "true";
            }
            else
            {
                var priceKey = (stickyType, isVerified) switch
                {
                    (Aethon.Shared.Enums.CreditType.StickyTop24h, true)  => SystemSettingKeys.StripePriceStickyVerified24h,
                    (Aethon.Shared.Enums.CreditType.StickyTop7d,  true)  => SystemSettingKeys.StripePriceStickyVerified7d,
                    (Aethon.Shared.Enums.CreditType.StickyTop30d, true)  => SystemSettingKeys.StripePriceStickyVerified30d,
                    (Aethon.Shared.Enums.CreditType.StickyTop24h, false) => SystemSettingKeys.StripePriceStickyUnverified24h,
                    (Aethon.Shared.Enums.CreditType.StickyTop7d,  false) => SystemSettingKeys.StripePriceStickyUnverified7d,
                    _                                                      => SystemSettingKeys.StripePriceStickyUnverified30d
                };
                var pid = await _settings.GetStringAsync(priceKey, ct);
                if (string.IsNullOrEmpty(pid))
                    return (false, null, "Sticky top product not yet configured. Please contact support.");
                lineItems.Add(new SessionLineItemOptions { Price = pid, Quantity = 1 });
            }
        }

        // ── No Stripe charge needed — publish immediately ─────────────────────
        if (lineItems.Count == 0)
        {
            ApplyAddOnsToJob(job, meta, request.StickyDuration, now);
            job.Status       = Aethon.Shared.Enums.JobStatus.Published;
            job.PublishedUtc ??= now;
            job.UpdatedUtc   = now;
            job.UpdatedByUserId = _currentUser.UserId;
            // Enforce expiry cap server-side
            var maxExpiry = isPremium ? now.AddDays(60) : now.AddDays(30);
            if (!job.PostingExpiresUtc.HasValue || job.PostingExpiresUtc.Value > maxExpiry)
                job.PostingExpiresUtc = maxExpiry;
            await _db.SaveChangesAsync(ct);
            return (true, null, null);
        }

        // ── Create Stripe Checkout Session ────────────────────────────────────
        var stripeClient = await GetStripeClientAsync(ct);
        var customerId   = await EnsureStripeCustomerAsync(org, stripeClient, ct);
        var webBase      = _config["Email:WebBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5200";

        var description = string.IsNullOrWhiteSpace(job.PoNumber)
            ? $"Job: {job.Title}"
            : $"Job: {job.Title} | PO: {job.PoNumber}";

        var sessionOptions = new SessionCreateOptions
        {
            Customer  = customerId,
            Mode      = "payment",
            LineItems = lineItems,
            Metadata  = meta,
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                SetupFutureUsage = "off_session",
                Description      = description,
                Metadata         = new Dictionary<string, string>
                {
                    ["job_id"]      = job.Id.ToString(),
                    ["po_number"]   = job.PoNumber ?? "",
                    ["job_title"]   = job.Title
                }
            },
            AllowPromotionCodes = true,
            SuccessUrl = $"{webBase}/app/jobs/{job.Id}/checkout-success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl  = $"{webBase}/app/jobs/{job.Id}"
        };

        // Set job on hold while awaiting Stripe confirmation
        job.Status     = Aethon.Shared.Enums.JobStatus.OnHold;
        job.UpdatedUtc = now;
        await _db.SaveChangesAsync(ct);

        var service = new SessionService(stripeClient);
        var session = await service.CreateAsync(sessionOptions, cancellationToken: ct);
        return (false, session.Url, null);
    }

    // Applies the add-on flags (stored in metadata dict) directly to the job entity.
    // Called when no Stripe charge is needed (all costs covered by credits).
    private static void ApplyAddOnsToJob(
        Aethon.Data.Entities.Job job,
        Dictionary<string, string> meta,
        int stickyDuration,
        DateTime now)
    {
        if (meta.TryGetValue("add_highlight", out var hl) && hl == "true")
        {
            job.IsHighlighted  = true;
            job.HighlightColour = meta.TryGetValue("highlight_colour", out var c) ? c : null;
        }
        if (meta.TryGetValue("add_video", out var vid) && vid == "true")
        {
            job.VideoYouTubeId = meta.TryGetValue("video_youtube_id", out var yt) && !string.IsNullOrEmpty(yt) ? yt : null;
            job.VideoVimeoId   = meta.TryGetValue("video_vimeo_id",   out var vm) && !string.IsNullOrEmpty(vm) ? vm : null;
        }
        if (meta.TryGetValue("add_ai_matching", out var ai) && ai == "true")
            job.HasAiCandidateMatching = true;
        if (stickyDuration > 0 && meta.TryGetValue("sticky_consumed_credit", out var sc) && sc == "true")
            job.StickyUntilUtc = now.AddDays(stickyDuration);
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

        var stripeClient = await GetStripeClientAsync(ct);
        var service = new global::Stripe.BillingPortal.SessionService(stripeClient);
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
        Aethon.Data.Entities.Organisation org, IStripeClient stripeClient, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(org.StripeCustomerId))
            return org.StripeCustomerId;

        var customerService = new CustomerService(stripeClient);
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
