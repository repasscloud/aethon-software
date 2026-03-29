namespace Aethon.Shared.Organisations;

public sealed class PublicTeamMemberDetailDto
{
    public string Slug { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? JobTitle { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? PublicEmail { get; set; }
    public string? PublicPhone { get; set; }
    public string? LinkedInUrl { get; set; }
    public bool IsIdentityVerified { get; set; }

    public string OrgName { get; set; } = null!;
    public string OrgSlug { get; set; } = null!;
}
