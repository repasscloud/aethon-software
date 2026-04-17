using Aethon.Shared.Enums;

namespace Aethon.Application.Applications.Commands.SubmitJobApplication;

public sealed class SubmitJobApplicationResult
{
    public Guid Id { get; init; }
    public Guid JobId { get; init; }
    public ApplicationStatus Status { get; init; }
    public DateTime SubmittedUtc { get; init; }
}
