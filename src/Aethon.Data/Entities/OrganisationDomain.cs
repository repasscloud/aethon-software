using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

public class OrganisationDomain : EntityBase
{
    public Guid OrganisationId { get; set; }
    public Organisation Organisation { get; set; } = null!;

    public string Domain { get; set; } = null!;
    public string NormalizedDomain { get; set; } = null!;
    public bool IsPrimary { get; set; }

    public DomainStatus Status { get; set; }
    public DomainVerificationMethod VerificationMethod { get; set; }
    public DomainTrustLevel TrustLevel { get; set; }

    public string? VerificationToken { get; set; }
    public string? VerificationDnsRecordName { get; set; }
    public string? VerificationDnsRecordValue { get; set; }
    public string? VerificationEmailAddress { get; set; }

    public DateTime? VerificationRequestedUtc { get; set; }
    public DateTime? VerifiedUtc { get; set; }
    public Guid? VerifiedByUserId { get; set; }
}