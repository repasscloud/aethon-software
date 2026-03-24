using System.ComponentModel.DataAnnotations;

namespace Aethon.Shared.Billing;

public sealed class VerificationCheckoutRequestDto
{
    /// <summary>"standard" or "enhanced"</summary>
    [Required]
    public string Tier { get; set; } = "";
}
