namespace Aethon.Data.Entities;

public sealed class JobSeekerCertificate : EntityBase
{
    public Guid JobSeekerProfileId { get; set; }
    public JobSeekerProfile Profile { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? IssuingOrganisation { get; set; }

    public int? IssuedMonth { get; set; }
    public int? IssuedYear { get; set; }
    public int? ExpiryYear { get; set; }

    public string? CredentialId { get; set; }
    public string? CredentialUrl { get; set; }
    public int SortOrder { get; set; }
}
