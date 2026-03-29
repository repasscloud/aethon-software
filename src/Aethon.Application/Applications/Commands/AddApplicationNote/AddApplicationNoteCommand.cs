namespace Aethon.Application.Applications.Commands.AddApplicationNote;

public sealed class AddApplicationNoteCommand
{
    public Guid ApplicationId { get; init; }
    public string Content { get; init; } = string.Empty;
}
