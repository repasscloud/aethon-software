namespace Aethon.Data.Entities;

public class SystemSetting
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = "";
    public string? Description { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public Guid? UpdatedByUserId { get; set; }
}
