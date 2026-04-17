using Aethon.Shared.Enums;

namespace Aethon.Shared.Jobs;

public sealed class EmployerJobApplicationDetailDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string JobTitle { get; set; } = "";
    public Guid ApplicantUserId { get; set; }
    public string ApplicantDisplayName { get; set; } = "";
    public string ApplicantEmail { get; set; } = "";
    public string Status { get; set; } = "";
    public string? StatusReason { get; set; }
    public string? CoverLetter { get; set; }
    public Guid? ResumeFileId { get; set; }
    public string? ResumeDownloadUrl { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
    public DateTime SubmittedUtc { get; set; }
    public DateTime? LastStatusChangedUtc { get; set; }
    public bool IsNotSuitable { get; set; }
    public string? NotSuitableReasons { get; set; }

    // ATS match result
    public AtsMatchStatus? AtsStatus { get; set; }
    public AtsMatchProvider? AtsProvider { get; set; }
    public int? AtsOverallScore { get; set; }
    public int? AtsSkillsScore { get; set; }
    public int? AtsExperienceScore { get; set; }
    public int? AtsLocationScore { get; set; }
    public int? AtsSalaryScore { get; set; }
    public int? AtsQualificationsScore { get; set; }
    public int? AtsWorkRightsScore { get; set; }
    public AtsMatchRecommendation? AtsRecommendation { get; set; }
    public string? AtsStrengths { get; set; }
    public string? AtsGaps { get; set; }
    public string? AtsSummary { get; set; }
    public float? AtsConfidence { get; set; }
    public string? AtsModelUsed { get; set; }
    public int? AtsTokensUsed { get; set; }
    public DateTime? AtsProcessedUtc { get; set; }
}