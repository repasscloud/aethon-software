using Aethon.Shared.Enums;

namespace Aethon.Application.Applications.Queries.GetApplicationById;

public sealed class ApplicationDetailModel
{
    public Guid Id { get; init; }
    public Guid JobId { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public Guid ApplicantUserId { get; init; }
    public string ApplicantDisplayName { get; init; } = string.Empty;
    public string ApplicantEmail { get; init; } = string.Empty;
    public ApplicationStatus Status { get; init; }
    public string? StatusReason { get; init; }
    public Guid? ResumeFileId { get; init; }
    public string? CoverLetter { get; init; }
    public string Source { get; init; } = string.Empty;
    public DateTime SubmittedUtc { get; init; }
    public DateTime? LastStatusChangedUtc { get; init; }
    public Guid OwnedByOrganisationId { get; init; }
    public Guid? ManagedByOrganisationId { get; init; }
}
