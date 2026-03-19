using Aethon.Shared.Enums;

namespace Aethon.Application.Applications.Commands.ScheduleInterview;

public sealed class ScheduleInterviewCommand
{
    public Guid ApplicationId { get; init; }
    public InterviewType Type { get; init; }
    public string? Title { get; init; }
    public string? Location { get; init; }
    public string? MeetingUrl { get; init; }
    public string? Notes { get; init; }
    public DateTime ScheduledStartUtc { get; init; }
    public DateTime ScheduledEndUtc { get; init; }
}
