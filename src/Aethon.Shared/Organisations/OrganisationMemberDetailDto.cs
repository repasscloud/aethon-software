namespace Aethon.Shared.Organisations;

public sealed class OrganisationMemberDetailDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsOwner { get; set; }
    public string? CompanyRole { get; set; }
    public string? RecruiterRole { get; set; }
    public string MembershipStatus { get; set; } = null!;
    public DateTime JoinedUtc { get; set; }
    public bool IsIdentityVerified { get; set; }
    public bool EmailConfirmed { get; set; }
    public OrganisationMemberProfileDto? Profile { get; set; }

    // Viewer context — describes what the caller is allowed to do
    public bool ViewerIsOwner { get; set; }
    public bool ViewerCanManage { get; set; }
    public string OrganisationType { get; set; } = null!;
}
