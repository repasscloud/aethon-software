using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Settings;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Aethon.Api.Infrastructure.Stripe;

/// <summary>
/// Charges the organisation's saved Stripe payment method for job listing add-ons
/// (highlight colour, AI matching) on already-published Standard jobs.
/// Premium jobs get these add-ons free — callers must guard accordingly.
/// </summary>
public sealed class JobAddonBillingService
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly ISystemSettingsService _settings;

    public JobAddonBillingService(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        ISystemSettingsService settings)
    {
        _db = db;
        _currentUser = currentUser;
        _settings = settings;
    }

    /// <summary>
    /// Charges the add-on price for <paramref name="priceSettingKey"/> to the org's saved card.
    /// Returns <c>(true, null)</c> on success or <c>(false, errorMessage)</c> on failure.
    /// Does NOT update the job — the caller applies the feature after a successful charge.
    /// </summary>
    public async Task<(bool Success, string? Error)> ChargeAddonAsync(
        Guid orgId,
        string priceSettingKey,
        string addonDescription,
        CancellationToken ct)
    {
        var org = await _db.Organisations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orgId, ct);

        if (org is null)
            return (false, "Organisation not found.");

        if (string.IsNullOrEmpty(org.StripeCustomerId))
            return (false, "No billing account found. Complete a purchase first to add a payment method.");

        var customerService = new CustomerService();
        Customer customer;
        try
        {
            customer = await customerService.GetAsync(
                org.StripeCustomerId,
                new CustomerGetOptions { Expand = ["invoice_settings.default_payment_method"] },
                cancellationToken: ct);
        }
        catch (StripeException ex)
        {
            return (false, $"Could not load billing account: {ex.Message}");
        }

        var paymentMethodId = customer.InvoiceSettings?.DefaultPaymentMethod?.Id
                           ?? customer.DefaultSourceId;

        if (string.IsNullOrEmpty(paymentMethodId))
            return (false, "No default payment method on file. Visit Billing to add a card.");

        var priceId = await _settings.GetStringAsync(priceSettingKey, ct);
        if (string.IsNullOrEmpty(priceId))
            return (false, $"Add-on product is not yet configured. Please contact support.");

        var priceService = new PriceService();
        Price price;
        try
        {
            price = await priceService.GetAsync(priceId, cancellationToken: ct);
        }
        catch (StripeException ex)
        {
            return (false, $"Could not retrieve price: {ex.Message}");
        }

        if (price.UnitAmount is null or 0)
            return (false, "Add-on price is not configured correctly. Please contact support.");

        var piService = new PaymentIntentService();
        PaymentIntent intent;
        try
        {
            intent = await piService.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount        = price.UnitAmount,
                Currency      = price.Currency,
                Customer      = org.StripeCustomerId,
                PaymentMethod = paymentMethodId,
                Confirm       = true,
                OffSession    = true,
                Description   = addonDescription,
                Metadata      = new Dictionary<string, string>
                {
                    ["organisation_id"] = orgId.ToString(),
                    ["purchase_type"]   = "addon",
                    ["addon"]           = addonDescription
                }
            }, cancellationToken: ct);
        }
        catch (StripeException ex)
        {
            return (false, $"Payment failed: {ex.Message}");
        }

        if (intent.Status != "succeeded")
            return (false, $"Payment did not succeed (status: {intent.Status}). Please update your payment method.");

        // Record the event for audit
        var now = DateTime.UtcNow;
        _db.StripePaymentEvents.Add(new StripePaymentEvent
        {
            Id             = Guid.NewGuid(),
            StripeEventId  = intent.Id,
            EventType      = "payment_intent.succeeded",
            AmountTotal    = intent.AmountReceived,
            Currency       = intent.Currency,
            PayloadJson    = $"{{\"payment_intent_id\":\"{intent.Id}\"}}",
            Status         = StripeEventStatus.Completed,
            OrganisationId = orgId,
            PurchaseType   = "addon",
            PriceId        = priceId,
            CompletedUtc   = now,
            CreatedUtc     = now,
            CreatedByUserId = _currentUser.IsAuthenticated ? _currentUser.UserId : null
        });

        await _db.SaveChangesAsync(ct);
        return (true, null);
    }

    /// <summary>
    /// Consumes a sticky credit for <paramref name="stickyType"/> or falls back to a Stripe charge.
    /// Returns <c>(true, null)</c> on success or <c>(false, errorMessage)</c> on failure.
    /// On success the credit is already decremented — no credit is granted; the caller applies sticky directly.
    /// </summary>
    public async Task<(bool Success, string? Error)> ConsumeOrChargeStickyAsync(
        Guid orgId,
        Guid jobId,
        CreditType stickyType,
        bool isVerified,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // Try credit first
        var credit = await _db.OrganisationJobCredits
            .Where(c =>
                c.OrganisationId == orgId &&
                c.CreditType == stickyType &&
                c.QuantityRemaining > 0 &&
                (c.ExpiresAt == null || c.ExpiresAt > now))
            .OrderBy(c => c.ExpiresAt)
            .FirstOrDefaultAsync(ct);

        if (credit is not null)
        {
            credit.QuantityRemaining--;
            _db.CreditConsumptionLogs.Add(new CreditConsumptionLog
            {
                Id                      = Guid.NewGuid(),
                OrganisationJobCreditId = credit.Id,
                OrganisationId          = orgId,
                JobId                   = jobId,
                ConsumedByUserId        = _currentUser.IsAuthenticated ? _currentUser.UserId : Guid.Empty,
                QuantityConsumed        = 1,
                ConsumedAt              = now,
                CreatedUtc              = now,
                CreatedByUserId         = _currentUser.IsAuthenticated ? _currentUser.UserId : null
            });
            await _db.SaveChangesAsync(ct);
            return (true, null);
        }

        // No credit — charge Stripe
        var priceKey = (stickyType, isVerified) switch
        {
            (CreditType.StickyTop24h, true)  => SystemSettingKeys.StripePriceStickyVerified24h,
            (CreditType.StickyTop7d,  true)  => SystemSettingKeys.StripePriceStickyVerified7d,
            (CreditType.StickyTop30d, true)  => SystemSettingKeys.StripePriceStickyVerified30d,
            (CreditType.StickyTop24h, false) => SystemSettingKeys.StripePriceStickyUnverified24h,
            (CreditType.StickyTop7d,  false) => SystemSettingKeys.StripePriceStickyUnverified7d,
            _                                => SystemSettingKeys.StripePriceStickyUnverified30d
        };

        var label = stickyType switch
        {
            CreditType.StickyTop24h => "Sticky top — 24 hours",
            CreditType.StickyTop7d  => "Sticky top — 7 days",
            _                       => "Sticky top — 30 days"
        };

        return await ChargeAddonAsync(orgId, priceKey, label, ct);
    }
}
