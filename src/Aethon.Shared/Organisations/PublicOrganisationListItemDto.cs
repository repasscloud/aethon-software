using Aethon.Shared.Enums;

namespace Aethon.Shared.Organisations;

public sealed class PublicOrganisationListItemDto
{
    public Guid OrganisationId { get; set; }
    public string Name { get; set; } = "";
    public string? Slug { get; set; }
    public string? LogoUrl { get; set; }
    public string? Summary { get; set; }
    public string? PublicLocationText { get; set; }
    public bool IsVerified { get; set; }
    public VerificationTier VerificationTier { get; set; }
    public JobCategory? Industry { get; set; }
    public CompanySize? CompanySize { get; set; }
    public int ActiveJobCount { get; set; }
}
