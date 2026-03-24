using Aethon.Shared.Enums;

namespace Aethon.Shared.Candidates;

public sealed class JobSeekerSkillDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public SkillLevel? SkillLevel { get; set; }
    public int SortOrder { get; set; }
}
