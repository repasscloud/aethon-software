namespace Aethon.Shared.Organisations;

public sealed class OrganisationProfileDto
{
    public string OrganisationId { get; set; } = "";
    public string OrganisationType { get; set; } = "";
    public string Name { get; set; } = "";
    public string? LegalName { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Slug { get; set; }
    public string? LogoUrl { get; set; }
    public string? Summary { get; set; }
    public string? PublicLocationText { get; set; }
    public string? PublicContactEmail { get; set; }
    public string? PublicContactPhone { get; set; }
    public bool IsPublicProfileEnabled { get; set; }
}
