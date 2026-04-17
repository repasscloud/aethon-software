namespace Aethon.Application.Applications.Commands.AttachApplicationFile;

public sealed class AttachApplicationFileCommand
{
    public Guid ApplicationId { get; init; }
    public Guid StoredFileId { get; init; }
    public string Category { get; init; } = "Attachment";
    public string? Notes { get; init; }
}
