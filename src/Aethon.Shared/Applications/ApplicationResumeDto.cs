namespace Aethon.Shared.Applications;

public sealed class ApplicationResumeDto
{
    public Guid Id { get; set; }
    public Guid StoredFileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long LengthBytes { get; set; }
}