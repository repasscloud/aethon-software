namespace Aethon.Shared.Files;

public sealed class UploadFileResultDto
{
    public string Id { get; set; } = "";
    public string OriginalFileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long LengthBytes { get; set; }
    public string DownloadUrl { get; set; } = "";
}
