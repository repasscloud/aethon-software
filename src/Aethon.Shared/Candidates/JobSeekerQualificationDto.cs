namespace Aethon.Shared.Candidates;

public sealed class JobSeekerQualificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Institution { get; set; }
    public int? CompletedYear { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}
