namespace Aethon.Data.Entities;

public sealed class JobSeekerQualification : EntityBase
{
    public Guid JobSeekerProfileId { get; set; }
    public JobSeekerProfile Profile { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string? Institution { get; set; }
    public int? CompletedYear { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}
