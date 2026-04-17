namespace Aethon.Shared.Organisations;

public sealed class PublicTeamMemberDto
{
    public string Slug { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? JobTitle { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsIdentityVerified { get; set; }
}
