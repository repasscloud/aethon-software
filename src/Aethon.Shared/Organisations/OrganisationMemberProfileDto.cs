namespace Aethon.Shared.Organisations;

public sealed class OrganisationMemberProfileDto
{
    public string? Slug { get; set; }
    public string? JobTitle { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? PublicEmail { get; set; }
    public string? PublicPhone { get; set; }
    public string? LinkedInUrl { get; set; }
    public bool IsPublicProfileEnabled { get; set; }
}
