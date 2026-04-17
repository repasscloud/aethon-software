using Aethon.Data.Identity;
using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

public class OrganisationClaimRequest : EntityBase
{
    public Guid OrganisationId { get; set; }
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