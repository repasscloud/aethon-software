namespace Aethon.Shared.AtsMatch;

/// <summary>
/// Full snapshot of a job and candidate, serialised into AtsMatchQueue.PayloadJson at enqueue time.
/// Workers deserialise this and send it to the LLM — no further DB queries needed.
/// </summary>
public sealed record AtsMatchPayload
{
    public AtsJobSnapshot Job { get; init; } = null!;
    public AtsCandidateSnapshot Candidate { get; init; } = null!;
}

public sealed record AtsJobSnapshot
{
    public string Title { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? Requirements { get; init; }
    public string? Benefits { get; init; }
    public List<string> Keywords { get; init; } = [];
    public string? Category { get; init; }
    public string EmploymentType { get; init; } = string.Empty;
    public string WorkplaceType { get; init; } = string.Empty;
    public string? Location { get; init; }
    public decimal? SalaryFrom { get; init; }
    public decimal? SalaryTo { get; init; }
    public string? SalaryCurrency { get; init; }
    public bool IsImmediateStart { get; init; }
    public AtsScreeningRequirements ScreeningRequirements { get; init; } = new();
}

public sealed record AtsScreeningRequirements
{
    public bool WorkRightsRequired { get; init; }
    public int? MinYearsExperience { get; init; }
    public bool PoliceCheckRequired { get; init; }
    public bool DriversLicenceRequired { get; init; }
    public List<string> QualificationsRequired { get; init; } = [];
}

public sealed record AtsCandidateSnapshot
{
    public string? Headline { get; init; }
    public string? Summary { get; init; }
    public string? ExperienceLevel { get; init; }
    public int? YearsOfExperience { get; init; }
    public List<AtsCandidateSkill> Skills { get; init; } = [];
    public List<AtsCandidateExperience> WorkExperience { get; init; } = [];
    public List<AtsCandidateQualification> Qualifications { get; init; } = [];
    public string? CurrentLocation { get; init; }
    public string? PreferredLocation { get; init; }
    public bool? WillingToRelocate { get; init; }
    public bool? HasWorkRights { get; init; }
    public bool? RequiresSponsorship { get; init; }
    public decimal? DesiredSalaryFrom { get; init; }
    public decimal? DesiredSalaryTo { get; init; }
    public string? DesiredSalaryCurrency { get; init; }
    public string? Availability { get; init; }
}

public sealed record AtsCandidateSkill(string Name, string? Level);

public sealed record AtsCandidateExperience(
    string Title,
    string? Company,
    int? StartYear,
    int? EndYear,
    bool IsCurrent,
    string? Summary);

public sealed record AtsCandidateQualification(
    string Name,
    string? Institution,
    int? Year);
