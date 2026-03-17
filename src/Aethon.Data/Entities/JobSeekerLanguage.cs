using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

public class JobSeekerLanguage : EntityBase
{
    public Guid JobSeekerProfileId { get; set; }
    public JobSeekerProfile JobSeekerProfile { get; set; } = null!;

    /// <summary>
    /// Language name.
    /// Example: English, Spanish, Arabic
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Whether this language applies to spoken, written, or both.
    /// </summary>
    public LanguageAbilityType AbilityType { get; set; }

    /// <summary>
    /// Optional proficiency level.
    /// </summary>
    public LanguageProficiencyLevel? ProficiencyLevel { get; set; }

    /// <summary>
    /// Whether this language record has been verified.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// When the language record was verified.
    /// </summary>
    public DateTime? VerifiedUtc { get; set; }

    /// <summary>
    /// Optional notes about the verification outcome.
    /// </summary>
    public string? VerificationNotes { get; set; }
}