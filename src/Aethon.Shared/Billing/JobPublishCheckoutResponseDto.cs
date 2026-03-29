namespace Aethon.Shared.Billing;

/// <summary>
/// Response from POST /api/v1/billing/job-publish-checkout.
/// </summary>
public sealed class JobPublishCheckoutResponseDto
{
    /// <summary>
    /// True when the job was published immediately because no payment was required
    /// (e.g. all costs covered by credits). CheckoutUrl will be null.
    /// </summary>
    public bool Published { get; set; }

    /// <summary>
    /// Stripe Checkout URL to redirect the user to. Null when Published = true.
    /// </summary>
    public string? CheckoutUrl { get; set; }
}
