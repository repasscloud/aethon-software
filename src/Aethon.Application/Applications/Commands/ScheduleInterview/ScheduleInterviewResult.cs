using Aethon.Shared.Enums;

namespace Aethon.Application.Applications.Commands.ScheduleInterview;

public sealed class ScheduleInterviewResult
{
    public Guid Id { get; init; }
    public Guid ApplicationId { get; init; }
    public InterviewType Type { get; init; }
    public InterviewStatus Status { get; init; }
    public string? Title { get; init; }
    public DateTime ScheduledStartUtc { get; init; }
    public DateTime ScheduledEndUtc { get; init; }
}
