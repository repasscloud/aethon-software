using Aethon.Shared.Enums;

namespace Aethon.Shared.Jobs;

public sealed class EmployerJobApplicationListItemDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid? ApplicantUserId { get; set; }
    public string? ApplicantDisplayName { get; set; }
    public string? ApplicantEmail { get; set; }
    public string Status { get; set; } = "";
    public string? Source { get; set; }
    public DateTime SubmittedUtc { get; set; }
    public DateTime? LastStatusChangedUtc { get; set; }
    public bool IsNotSuitable { get; set; }

    // ATS match summary
    public AtsMatchStatus? AtsStatus { get; set; }
    public AtsMatchProvider? AtsProvider { get; set; }
    public int? AtsOverallScore { get; set; }
    public AtsMatchRecommendation? AtsRecommendation { get; set; }
}
