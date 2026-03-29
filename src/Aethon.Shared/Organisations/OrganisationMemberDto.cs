namespace Aethon.Shared.Organisations;

public sealed class OrganisationMemberDto
{
    public string UserId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsOwner { get; set; }
    public string MembershipStatus { get; set; } = "";
    public string? CompanyRole { get; set; }
    public string? RecruiterRole { get; set; }
    public DateTime JoinedUtc { get; set; }
}
