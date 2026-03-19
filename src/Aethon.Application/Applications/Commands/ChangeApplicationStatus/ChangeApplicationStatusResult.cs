using Aethon.Shared.Enums;

namespace Aethon.Application.Applications.Commands.ChangeApplicationStatus;

public sealed class ChangeApplicationStatusResult
{
    public Guid Id { get; init; }
    public ApplicationStatus Status { get; init; }
    public string? StatusReason { get; init; }
    public DateTime? LastStatusChangedUtc { get; init; }
}