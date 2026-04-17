using System.ComponentModel.DataAnnotations;
using Aethon.Shared.Enums;

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

    [MaxLength(1000)]
    public string? BannerImageUrl { get; set; }

    [MaxLength(4000)]
    public string? Summary { get; set; }

    [MaxLength(250)]
    public string? PublicLocationText { get; set; }

    [MaxLength(150)]
    public string? LocationCity { get; set; }
    [MaxLength(150)]
    public string? LocationState { get; set; }
    [MaxLength(100)]
    public string? LocationCountry { get; set; }
    [MaxLength(10)]
    public string? LocationCountryCode { get; set; }
    public double? LocationLatitude { get; set; }
    public double? LocationLongitude { get; set; }
    [MaxLength(500)]
    public string? LocationPlaceId { get; set; }

    [MaxLength(200)]
    public string? PrimaryContactName { get; set; }

    [MaxLength(320)]
    [EmailAddress]
    public string? PrimaryContactEmail { get; set; }

    [MaxLength(10)]
    public string? PrimaryContactPhoneDialCode { get; set; }

    [MaxLength(50)]
    public string? PrimaryContactPhone { get; set; }

    [MaxLength(320)]
    [EmailAddress]
    public string? PublicContactEmail { get; set; }

    [MaxLength(10)]
    public string? PublicContactPhoneDialCode { get; set; }

    [MaxLength(50)]
    public string? PublicContactPhone { get; set; }

    // Legal & verification
    [MaxLength(500)]
    public string? RegisteredAddressLine1 { get; set; }

    [MaxLength(500)]
    public string? RegisteredAddressLine2 { get; set; }

    [MaxLength(150)]
    public string? RegisteredCity { get; set; }

    [MaxLength(150)]
    public string? RegisteredState { get; set; }

    [MaxLength(20)]
    public string? RegisteredPostcode { get; set; }

    [MaxLength(150)]
    public string? RegisteredCountry { get; set; }

    [MaxLength(10)]
    public string? RegisteredCountryCode { get; set; }

    [MaxLength(100)]
    public string? TaxRegistrationNumber { get; set; }

    [MaxLength(100)]
    public string? BusinessRegistrationNumber { get; set; }

    public bool IsPublicProfileEnabled { get; set; }
    public bool IsEqualOpportunityEmployer { get; set; }
    public bool IsAccessibleWorkplace { get; set; }

    public CompanySize? CompanySize { get; set; }
    public JobCategory? Industry { get; set; }

    [MaxLength(500)]
    public string? LinkedInUrl { get; set; }

    [MaxLength(100)]
    public string? TwitterHandle { get; set; }

    [MaxLength(500)]
    public string? FacebookUrl { get; set; }

    [MaxLength(100)]
    public string? TikTokHandle { get; set; }

    [MaxLength(100)]
    public string? InstagramHandle { get; set; }

    [MaxLength(500)]
    public string? YouTubeUrl { get; set; }
}
