using Aethon.Shared.Enums;

namespace Aethon.Shared.Applications;

public sealed class ApplicationSummaryDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public Guid ApplicantUserId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public Guid? AssignedRecruiterUserId { get; set; }
    public DateTime CreatedUtc { get; set; }
}