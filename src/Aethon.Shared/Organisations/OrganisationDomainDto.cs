using Aethon.Shared.Enums;

namespace Aethon.Shared.Organisations;

public sealed class OrganisationDomainDto
{
    public Guid Id { get; set; }
    public Guid OrganisationId { get; set; }
    public string Domain { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public DomainStatus Status { get; set; }
    public DomainVerificationMethod VerificationMethod { get; set; }
    public DomainTrustLevel TrustLevel { get; set; }
    public string? VerificationToken { get; set; }
    public string? VerificationDnsRecordName { get; set; }
    public string? VerificationDnsRecordValue { get; set; }
    public string? VerificationEmailAddress { get; set; }
    public DateTime? VerifiedUtc { get; set; }
}
