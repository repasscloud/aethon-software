namespace Aethon.Shared.Organisations;

public sealed class MyMemberProfileDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public bool IsIdentityVerified { get; set; }

    /// <summary>null = no request ever submitted; otherwise Pending | Approved | Denied</summary>
    public string? VerificationRequestStatus { get; set; }

    public Guid OrganisationId { get; set; }
    public string OrgName { get; set; } = null!;
    public string? OrgSlug { get; set; }
    public bool OrgIsPublicProfileEnabled { get; set; }

    public OrganisationMemberProfileDto? Profile { get; set; }
}
