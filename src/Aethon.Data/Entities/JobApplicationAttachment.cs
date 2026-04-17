namespace Aethon.Data.Entities;

public sealed class JobApplicationAttachment
{
    public Guid Id { get; set; }

    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public Guid StoredFileId { get; set; }
    public StoredFile StoredFile { get; set; } = null!;

    public string Category { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public DateTime CreatedUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
}
