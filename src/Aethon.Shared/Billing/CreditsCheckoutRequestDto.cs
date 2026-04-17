using System.ComponentModel.DataAnnotations;

namespace Aethon.Shared.Billing;

public sealed class CreditsCheckoutRequestDto
{
    /// <summary>SystemSettingKeys constant for the Stripe price to use, e.g. "Stripe.Price.Job.Standard.5x"</summary>
    [Required]
    public string PriceKey { get; set; } = "";
}
