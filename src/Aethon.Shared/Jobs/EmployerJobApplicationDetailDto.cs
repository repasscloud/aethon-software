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
    public string? CoverLetter { get; set; }
    public Guid? ResumeFileId { get; set; }
    public string? ResumeDownloadUrl { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
    public DateTime SubmittedUtc { get; set; }
    public DateTime? LastStatusChangedUtc { get; set; }
}