using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Settings;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Aethon.Api.Infrastructure.Stripe;

/// <summary>
/// Charges the organisation's saved Stripe payment method for a job posting credit
/// when no credits are available in the ledger.
/// On success a single-use credit is granted so that <see cref="Aethon.Application.Jobs.Commands.PublishJob.PublishJobHandler"/>
/// can consume it in its normal credit-check path.
/// </summary>
public sealed class JobPublishBillingService
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly ISystemSettingsService _settings;

    public JobPublishBillingService(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        ISystemSettingsService settings)
    {
        _db = db;
        _currentUser = currentUser;
        _settings = settings;
    }

    /// <summary>
    /// Charges the organisation's saved Stripe payment method for one job posting credit
    /// of the given <paramref name="creditType"/>, then grants that credit so the publish
    /// handler can consume it immediately.
    /// Returns <c>(true, null)</c> on success or <c>(false, errorMessage)</c> on failure.
    /// </summary>
    public async Task<(bool Success, string? Error)> ChargeAndGrantPostingCreditAsync(
        Guid orgId,
        CreditType creditType,
        CancellationToken ct)
    {
        var org = await _db.Organisations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orgId, ct);

        if (org is null)
            return (false, "Organisation not found.");

        if (string.IsNullOrEmpty(org.StripeCustomerId))
            return (false, "No billing account found. Complete a purchase first to add a payment method.");

        // Retrieve the customer's default payment method
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

        // Look up the 1x price for this credit type
        var priceKey = creditType == CreditType.JobPostingPremium
            ? SystemSettingKeys.StripePriceJobPremium1x
            : SystemSettingKeys.StripePriceJobStandard1x;

        var priceId = await _settings.GetStringAsync(priceKey, ct);
        if (string.IsNullOrEmpty(priceId))
            return (false, "Job posting product is not yet configured. Please contact support.");

        // Retrieve price to get unit amount
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
            return (false, "Price amount is not configured correctly. Please contact support.");

        // Create an off-session PaymentIntent against the saved card
        var piService = new PaymentIntentService();
        PaymentIntent intent;
        try
        {
            intent = await piService.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount               = price.UnitAmount,
                Currency             = price.Currency,
                Customer             = org.StripeCustomerId,
                PaymentMethod        = paymentMethodId,
                Confirm              = true,
                OffSession           = true,
                Description          = $"1× {(creditType == CreditType.JobPostingPremium ? "Premium" : "Standard")} job posting credit",
                Metadata             = new Dictionary<string, string>
                {
                    ["organisation_id"] = orgId.ToString(),
                    ["credit_type"]     = creditType.ToString(),
                    ["purchase_type"]   = "job_credits"
                }
            }, cancellationToken: ct);
        }
        catch (StripeException ex)
        {
            return (false, $"Payment failed: {ex.Message}");
        }

        if (intent.Status != "succeeded")
            return (false, $"Payment did not succeed (status: {intent.Status}). Please update your payment method.");

        // Record the payment event for audit/admin
        var now = DateTime.UtcNow;
        var stripeEvent = new StripePaymentEvent
        {
            Id            = Guid.NewGuid(),
            StripeEventId = intent.Id,
            EventType     = "payment_intent.succeeded",
            AmountTotal   = intent.AmountReceived,
            Currency      = intent.Currency,
            PayloadJson   = $"{{\"payment_intent_id\":\"{intent.Id}\"}}",
            Status        = StripeEventStatus.Completed,
            OrganisationId = orgId,
            PurchaseType  = "job_credits",
            PriceId       = priceId,
            CompletedUtc  = now,
            CreatedUtc    = now,
            CreatedByUserId = _currentUser.IsAuthenticated ? _currentUser.UserId : null
        };
        _db.StripePaymentEvents.Add(stripeEvent);

        // Grant a single-use credit so the publish handler can consume it
        _db.OrganisationJobCredits.Add(new OrganisationJobCredit
        {
            Id                  = Guid.NewGuid(),
            OrganisationId      = orgId,
            CreditType          = creditType,
            Source              = CreditSource.StripePurchase,
            QuantityOriginal    = 1,
            QuantityRemaining   = 1,
            ExpiresAt           = null, // no expiry — already paid
            StripePaymentEventId = stripeEvent.Id,
            CreatedUtc          = now,
            CreatedByUserId     = _currentUser.IsAuthenticated ? _currentUser.UserId : null
        });

        await _db.SaveChangesAsync(ct);
        return (true, null);
    }
}
