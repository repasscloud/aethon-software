namespace Aethon.Shared.Organisations;

public sealed class OrganisationInviteDto
{
    public Guid InvitationId { get; set; }
    public string Email { get; set; } = "";
    public string InvitationType { get; set; } = "";
    public string InvitationStatus { get; set; } = "";
    public string? CompanyRole { get; set; }
    public string? RecruiterRole { get; set; }
    public bool AllowClaimAsOwner { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public string Token { get; set; } = "";
}
