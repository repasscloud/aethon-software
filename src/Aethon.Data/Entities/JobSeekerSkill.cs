using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

public sealed class JobSeekerSkill : EntityBase
{
    public Guid JobSeekerProfileId { get; set; }
    public JobSeekerProfile Profile { get; set; } = null!;

    public string Name { get; set; } = null!;
    public SkillLevel? SkillLevel { get; set; }
    public int SortOrder { get; set; }
}
