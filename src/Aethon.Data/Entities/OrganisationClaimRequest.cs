using Aethon.Data.Enums;
using Aethon.Data.Identity;

namespace Aethon.Data.Entities;

public class OrganisationClaimRequest : EntityBase
{
    public string OrganisationId { get; set; } = null!;
    public Organisation Organisation { get; set; } = null!;

    public Guid RequestedByUserId { get; set; }
    public ApplicationUser RequestedByUser { get; set; } = null!;

    public string EmailUsed { get; set; } = null!;
    public string EmailDomain { get; set; } = null!;

    public ClaimRequestStatus Status { get; set; }
    public DomainVerificationMethod VerificationMethod { get; set; }

    public string? VerificationToken { get; set; }
    public DateTime? VerifiedUtc { get; set; }

    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedUtc { get; set; }
    public string? RejectionReason { get; set; }
}