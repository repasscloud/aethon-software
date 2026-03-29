namespace Aethon.Shared.Applications;

public sealed class ApplicationTimelineItemDto
{
    public string EventType { get; set; } = "";
    public DateTime OccurredUtc { get; set; }

    public Guid? PerformedByUserId { get; set; }
    public string? PerformedByDisplayName { get; set; }

    public string Title { get; set; } = "";
    public string? Description { get; set; }

    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
}
