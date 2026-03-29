using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

/// <summary>
/// AI-generated analysis result for a candidate resume.
/// Created in Pending state when a resume is uploaded; processed asynchronously.
/// </summary>
public sealed class ResumeAnalysis : EntityBase
{
    public Guid JobSeekerResumeId { get; set; }
    public JobSeekerResume JobSeekerResume { get; set; } = null!;

    public Guid StoredFileId { get; set; }
    public StoredFile StoredFile { get; set; } = null!;

    public ResumeAnalysisStatus Status { get; set; } = ResumeAnalysisStatus.Pending;

    /// <summary>Suggested professional headline extracted from the resume.</summary>
    public string? HeadlineSuggestion { get; set; }

    /// <summary>AI-generated 2-3 sentence summary of the candidate.</summary>
    public string? SummaryExtract { get; set; }

    /// <summary>JSON array of extracted skills, e.g. ["C#", ".NET", "Azure"].</summary>
    public string? SkillsJson { get; set; }

    /// <summary>Suggested seniority level: Junior, Mid, Senior, Lead, Executive.</summary>
    public string? ExperienceLevel { get; set; }

    /// <summary>Estimated total years of professional experience.</summary>
    public int? YearsExperience { get; set; }

    /// <summary>When the analysis completed successfully.</summary>
    public DateTime? AnalysedUtc { get; set; }

    /// <summary>Error message if the analysis failed.</summary>
    public string? AnalysisError { get; set; }
}
