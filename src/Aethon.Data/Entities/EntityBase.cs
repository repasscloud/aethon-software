namespace Aethon.Data.Entities;

public abstract class EntityBase
{
    public string Id { get; set; } = null!;
    public DateTime CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
    public string? CreatedByUserId { get; set; }
    public string? UpdatedByUserId { get; set; }
}