using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

/// <summary>
/// Result of an AI ATS match evaluation for a job application.
/// One row per application (the most recent completed match wins).
/// </summary>
public sealed class AtsMatchResult : EntityBase
{
    public Guid AtsMatchQueueItemId { get; set; }
    public AtsMatchQueueItem QueueItem { get; set; } = null!;

    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public Guid JobId { get; set; }
    public Guid CandidateUserId { get; set; }

    public AtsMatchProvider Provider { get; set; }

    /// <summary>Specific model version used, e.g. "claude-sonnet-4-6" or "mistral:7b".</summary>
    public string ModelUsed { get; set; } = string.Empty;

    /// <summary>Overall match score 0–100.</summary>
    public int OverallScore { get; set; }

    // Per-dimension scores (null if the dimension could not be evaluated due to missing data)
    public int? SkillsScore { get; set; }
    public int? ExperienceScore { get; set; }
    public int? LocationScore { get; set; }
    public int? SalaryScore { get; set; }
    public int? QualificationsScore { get; set; }
    public int? WorkRightsScore { get; set; }

    public AtsMatchRecommendation Recommendation { get; set; }

    /// <summary>JSON array of strength strings, e.g. ["Strong C# alignment", "Located in city"].</summary>
    public string? Strengths { get; set; }

    /// <summary>JSON array of gap strings, e.g. ["Salary expectation above range"].</summary>
    public string? Gaps { get; set; }

    /// <summary>2-3 sentence plain-English summary of the candidate's suitability.</summary>
    public string? Summary { get; set; }

    /// <summary>Model confidence 0.0–1.0.</summary>
    public float? Confidence { get; set; }

    /// <summary>Raw JSON response from the LLM (retained for debugging and audit).</summary>
    public string? RawResponseJson { get; set; }

    /// <summary>Approximate input + output tokens used. Populated for Claude calls; null for Ollama.</summary>
    public int? TokensUsed { get; set; }

    public DateTime ProcessedUtc { get; set; }
}
