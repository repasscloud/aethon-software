using Aethon.Shared.Enums;

namespace Aethon.Shared.Applications;

public sealed class ApplicationDetailDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;

    public Guid ApplicantUserId { get; set; }
    public string ApplicantDisplayName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;

    public ApplicationStatus Status { get; set; }
    public string? StatusReason { get; set; }

    public Guid? ResumeFileId { get; set; }
    public string? ResumeDownloadUrl { get; set; }
    public ApplicationResumeDto? Resume { get; set; }

    public string? CoverLetter { get; set; }
    public string Source { get; set; } = string.Empty;

    public DateTime SubmittedUtc { get; set; }
    public DateTime? LastStatusChangedUtc { get; set; }
    public DateTime? LastActivityUtc { get; set; }

    public Guid? AssignedRecruiterUserId { get; set; }
    public string? AssignedRecruiterDisplayName { get; set; }

    public decimal? Rating { get; set; }
    public string? Recommendation { get; set; }

    public bool IsRejected { get; set; }
    public bool IsWithdrawn { get; set; }
    public bool IsHired { get; set; }
}