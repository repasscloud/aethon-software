using System.ComponentModel.DataAnnotations;

namespace Aethon.Shared.Organisations;

public sealed class SubmitClaimRequestDto
{
    [Required]
    [MaxLength(200)]
    public string OrganisationSlug { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
