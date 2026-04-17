namespace Aethon.Shared.Applications;

public sealed class ApplicationFileDto
{
    public Guid Id { get; set; }
    public Guid StoredFileId { get; set; }

    public string Category { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long LengthBytes { get; set; }

    public DateTime CreatedUtc { get; set; }
}
