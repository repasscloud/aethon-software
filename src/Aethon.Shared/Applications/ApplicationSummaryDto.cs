using Aethon.Shared.Enums;

namespace Aethon.Shared.Applications;

public sealed class ApplicationSummaryDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;

    // Applicant info (populated in employer/recruiter views)
    public Guid? ApplicantUserId { get; set; }
    public string? ApplicantDisplayName { get; set; }
    public string? ApplicantEmail { get; set; }

    public ApplicationStatus Status { get; set; }
    public string? StatusReason { get; set; }

    public string Source { get; set; } = string.Empty;

    public Guid? ResumeFileId { get; set; }
    public ApplicationResumeDto? Resume { get; set; }

    public DateTime SubmittedUtc { get; set; }
    public DateTime? LastStatusChangedUtc { get; set; }
    public DateTime? LastActivityUtc { get; set; }

    public bool IsRejected { get; set; }
    public bool IsWithdrawn { get; set; }
    public bool IsHired { get; set; }
    public bool IsNotSuitable { get; set; }

    // ATS match summary (populated once the background worker completes)
    public AtsMatchStatus? AtsStatus { get; set; }
    public AtsMatchProvider? AtsProvider { get; set; }
    public int? AtsOverallScore { get; set; }
    public AtsMatchRecommendation? AtsRecommendation { get; set; }
}