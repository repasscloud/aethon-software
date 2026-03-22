namespace Aethon.Shared.Jobs;

/// <summary>
/// Employer-configured screening questions for a job posting.
/// Stored as JSON on the Job entity.
/// </summary>
public class ScreeningConfig
{
    /// <summary>
    /// When true, applications that fail one or more Must-have questions are
    /// automatically tagged as Not Suitable (but never rejected).
    /// </summary>
    public bool AutoTagNotSuitable { get; set; }

    public ScreeningQuestion WorkRights { get; set; } = new();
    public ScreeningQuestion YearsExperience { get; set; } = new();
    public SalaryScreeningQuestion Salary { get; set; } = new();
    public ScreeningQuestion NoticePeriod { get; set; } = new();
    public ScreeningQuestion PoliceCheck { get; set; } = new();
    public ScreeningQuestion WorkingWithChildren { get; set; } = new();
    public ScreeningQuestion MedicalCheck { get; set; } = new();
    public ScreeningQuestion DriversLicence { get; set; } = new();
    public ScreeningQuestion CarAccess { get; set; } = new();
    public ScreeningQuestion PublicHolidays { get; set; } = new();
    public ScreeningQuestion Qualification { get; set; } = new();
}

public class ScreeningQuestion
{
    /// <summary>Controls whether this question appears on the application form.</summary>
    public bool Enabled { get; set; }

    /// <summary>Controls suitability evaluation (not visibility).</summary>
    public bool IsMustHave { get; set; }

    /// <summary>
    /// The answer values the employer considers acceptable.
    /// Only evaluated when IsMustHave is true.
    /// </summary>
    public List<string> AcceptableAnswers { get; set; } = [];
}

public class SalaryScreeningQuestion : ScreeningQuestion
{
    /// <summary>
    /// Maximum salary the employer can offer for this role.
    /// An applicant whose minimum expected salary exceeds this value will be
    /// considered a mismatch on a Must-have salary question.
    /// </summary>
    public string? AcceptableMaxSalary { get; set; }
}

/// <summary>
/// Applicant's answers to the enabled screening questions.
/// Stored as JSON on the JobApplication entity.
/// </summary>
public class ScreeningAnswers
{
    public string? WorkRights { get; set; }
    public string? YearsExperience { get; set; }
    public string? SalaryMin { get; set; }
    public string? SalaryMax { get; set; }
    public string? NoticePeriod { get; set; }
    public string? PoliceCheck { get; set; }
    public string? WorkingWithChildren { get; set; }
    public string? MedicalCheck { get; set; }
    public string? DriversLicence { get; set; }
    public string? CarAccess { get; set; }
    public string? PublicHolidays { get; set; }
    public List<string> Qualification { get; set; } = [];
}
