using Aethon.Shared.Enums;

namespace Aethon.Shared.Organisations;

public sealed class OrganisationProfileDto
{
    public Guid OrganisationId { get; set; }
    public string OrganisationType { get; set; } = "";
    public string Name { get; set; } = "";
    public string? LegalName { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Slug { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerImageUrl { get; set; }
    public string? Summary { get; set; }
    public string? PublicLocationText { get; set; }
    public string? LocationCity { get; set; }
    public string? LocationState { get; set; }
    public string? LocationCountry { get; set; }
    public string? LocationCountryCode { get; set; }
    public double? LocationLatitude { get; set; }
    public double? LocationLongitude { get; set; }
    public string? LocationPlaceId { get; set; }
    public string? PrimaryContactName { get; set; }
    public string? PrimaryContactEmail { get; set; }
    public string? PrimaryContactPhoneDialCode { get; set; }
    public string? PrimaryContactPhone { get; set; }
    public string? PublicContactEmail { get; set; }
    public string? PublicContactPhoneDialCode { get; set; }
    public string? PublicContactPhone { get; set; }

    // Legal & verification
    public string? RegisteredAddressLine1 { get; set; }
    public string? RegisteredAddressLine2 { get; set; }
    public string? RegisteredCity { get; set; }
    public string? RegisteredState { get; set; }
    public string? RegisteredPostcode { get; set; }
    public string? RegisteredCountry { get; set; }
    public string? RegisteredCountryCode { get; set; }
    public string? TaxRegistrationNumber { get; set; }
    public string? BusinessRegistrationNumber { get; set; }
    public bool IsPublicProfileEnabled { get; set; }
    public bool IsEqualOpportunityEmployer { get; set; }
    public bool IsAccessibleWorkplace { get; set; }
    public bool IsVerified { get; set; }
    public VerificationTier VerificationTier { get; set; }
    public CompanySize? CompanySize { get; set; }
    public JobCategory? Industry { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TwitterHandle { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TikTokHandle { get; set; }
    public string? InstagramHandle { get; set; }
    public string? YouTubeUrl { get; set; }
}
