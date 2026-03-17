using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

public class OrganisationInvitation : EntityBase
{
    public InvitationType Type { get; set; }
    public InvitationStatus Status { get; set; }

    public string OrganisationId { get; set; } = null!;
    public Organisation Organisation { get; set; } = null!;

    public string Email { get; set; } = null!;
    public string NormalizedEmail { get; set; } = null!;
    public string EmailDomain { get; set; } = null!;

    public string Token { get; set; } = null!;
    public DateTime ExpiresUtc { get; set; }

    public CompanyRole? CompanyRole { get; set; }
    public RecruiterRole? RecruiterRole { get; set; }

    public bool AllowClaimAsOwner { get; set; }
    public Guid? AcceptedByUserId { get; set; }
    public DateTime? AcceptedUtc { get; set; }
}