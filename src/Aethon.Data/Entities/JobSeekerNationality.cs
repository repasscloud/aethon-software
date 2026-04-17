namespace Aethon.Data.Entities;

public class JobSeekerNationality : EntityBase
{
    public Guid JobSeekerProfileId { get; set; }
    public JobSeekerProfile JobSeekerProfile { get; set; } = null!;

    /// <summary>
    /// Nationality/citizenship name.
    /// Example: Australian, New Zealand, British
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Whether this nationality has been verified.
    /// Verification process can be manual now, external later.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// When the nationality was verified.
    /// </summary>
    public DateTime? VerifiedUtc { get; set; }

    /// <summary>
    /// Optional notes about the verification outcome.
    /// </summary>
    public string? VerificationNotes { get; set; }
}