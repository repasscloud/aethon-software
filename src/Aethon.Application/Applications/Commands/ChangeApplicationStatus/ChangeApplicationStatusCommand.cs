using Aethon.Shared.Enums;

namespace Aethon.Application.Applications.Commands.ChangeApplicationStatus;

public sealed class ChangeApplicationStatusCommand
{
    public Guid ApplicationId { get; init; }
    public ApplicationStatus Status { get; init; }

    public string? Reason { get; init; }
    public string? Notes { get; init; }
}