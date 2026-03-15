namespace Aethon.Shared.Jobs;

public sealed class EmployerJobApplicationListItemDto
{
    public string Id { get; set; } = "";
    public string JobId { get; set; } = "";
    public string ApplicantUserId { get; set; } = "";
    public string ApplicantDisplayName { get; set; } = "";
    public string ApplicantEmail { get; set; } = "";
    public string Status { get; set; } = "";
    public string? Source { get; set; }
    public DateTime SubmittedUtc { get; set; }
    public DateTime? LastStatusChangedUtc { get; set; }
}
