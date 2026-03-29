namespace Aethon.Data.Entities;

public sealed class JobSeekerWorkExperience : EntityBase
{
    public Guid JobSeekerProfileId { get; set; }
    public JobSeekerProfile Profile { get; set; } = null!;

    public string JobTitle { get; set; } = null!;
    public string EmployerName { get; set; } = null!;

    public int StartMonth { get; set; }
    public int StartYear { get; set; }

    public int? EndMonth { get; set; }
    public int? EndYear { get; set; }

    /// <summary>When true, EndMonth/EndYear are ignored — this is the current role.</summary>
    public bool IsCurrent { get; set; }

    public string? Description { get; set; }

    /// <summary>Display order within the profile.</summary>
    public int SortOrder { get; set; }
}
