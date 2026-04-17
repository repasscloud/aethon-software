namespace Aethon.Shared.Files;

public sealed class FileUploadResultDto
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long LengthBytes { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}
