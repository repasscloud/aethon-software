using Aethon.Data.Identity;
using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

public class OrganisationMembership : EntityBase
{
    public Guid OrganisationId { get; set; }
    public Organisation Organisation { get; set; } = null!;

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public MembershipStatus Status { get; set; }

    public CompanyRole? CompanyRole { get; set; }
    public RecruiterRole? RecruiterRole { get; set; }

    public bool IsOwner { get; set; }
    public DateTime JoinedUtc { get; set; }
    public DateTime? LeftUtc { get; set; }
}