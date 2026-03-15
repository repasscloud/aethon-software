using System.ComponentModel.DataAnnotations;

namespace Aethon.Shared.Organisations;

public sealed class UpdateOrganisationProfileRequestDto
{
    [Required]
    [MaxLength(250)]
    public string Name { get; set; } = "";

    [MaxLength(250)]
    public string? LegalName { get; set; }

    [MaxLength(500)]
    public string? WebsiteUrl { get; set; }

    [MaxLength(100)]
    public string? Slug { get; set; }

    [MaxLength(1000)]
    public string? LogoUrl { get; set; }

    [MaxLength(4000)]
    public string? Summary { get; set; }

    [MaxLength(250)]
    public string? PublicLocationText { get; set; }

    [MaxLength(320)]
    [EmailAddress]
    public string? PublicContactEmail { get; set; }

    [MaxLength(50)]
    public string? PublicContactPhone { get; set; }

    public bool IsPublicProfileEnabled { get; set; }
}
