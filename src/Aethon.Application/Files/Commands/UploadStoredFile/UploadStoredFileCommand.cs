namespace Aethon.Application.Files.Commands.UploadStoredFile;

public sealed class UploadStoredFileCommand
{
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = "application/octet-stream";
    public byte[] Content { get; init; } = [];
}
