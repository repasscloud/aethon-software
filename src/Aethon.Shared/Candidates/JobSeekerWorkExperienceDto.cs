namespace Aethon.Shared.Candidates;

public sealed class JobSeekerWorkExperienceDto
{
    public Guid Id { get; set; }
    public string JobTitle { get; set; } = null!;
    public string EmployerName { get; set; } = null!;
    public int StartMonth { get; set; }
    public int StartYear { get; set; }
    public int? EndMonth { get; set; }
    public int? EndYear { get; set; }
    public bool IsCurrent { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}
