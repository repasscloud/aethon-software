using Aethon.Data.Enums;
using Aethon.Data.Identity;

namespace Aethon.Data.Entities;

public class OrganisationMembership : EntityBase
{
    public string OrganisationId { get; set; } = null!;
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