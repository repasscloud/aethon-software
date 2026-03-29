using System.ComponentModel.DataAnnotations;

namespace Aethon.Shared.Organisations;

public sealed class ConfirmDomainVerificationRequestDto
{
    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;
}
