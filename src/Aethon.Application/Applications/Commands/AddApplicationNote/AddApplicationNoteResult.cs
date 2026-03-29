namespace Aethon.Application.Applications.Commands.AddApplicationNote;

public sealed class AddApplicationNoteResult
{
    public Guid Id { get; init; }
    public Guid ApplicationId { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedUtc { get; init; }
}
