namespace Aethon.Data.Entities;

public sealed class StoredFile : EntityBase
{
    public string FileName { get; set; } = null!;
    public string OriginalFileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long LengthBytes { get; set; }

    public string StorageProvider { get; set; } = null!;
    public string StoragePath { get; set; } = null!;

    public Guid UploadedByUserId { get; set; }
}
