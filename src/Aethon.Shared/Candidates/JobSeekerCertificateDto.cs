namespace Aethon.Shared.Candidates;

public sealed class JobSeekerCertificateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? IssuingOrganisation { get; set; }
    public int? IssuedMonth { get; set; }
    public int? IssuedYear { get; set; }
    public int? ExpiryYear { get; set; }
    public string? CredentialId { get; set; }
    public string? CredentialUrl { get; set; }
    public int SortOrder { get; set; }
}
