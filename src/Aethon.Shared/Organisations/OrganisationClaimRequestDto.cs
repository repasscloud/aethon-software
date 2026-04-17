using Aethon.Shared.Enums;

namespace Aethon.Shared.Organisations;

public sealed class OrganisationClaimRequestDto
{
    public Guid Id { get; set; }
    public Guid OrganisationId { get; set; }
    public string OrganisationName { get; set; } = string.Empty;
    public string OrganisationSlug { get; set; } = string.Empty;
    public ClaimRequestStatus Status { get; set; }
    public DomainVerificationMethod VerificationMethod { get; set; }
    public string? VerificationToken { get; set; }
    public DateTime? VerifiedUtc { get; set; }
    public DateTime SubmittedUtc { get; set; }
    public string? RejectionReason { get; set; }
}
