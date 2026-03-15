using Aethon.Shared.Enums;
using Aethon.Shared.Files;

namespace Aethon.Shared.Jobs;

public sealed class JobSeekerProfileDto
{
    public string UserId { get; set; } = "";
    public string? Headline { get; set; }
    public string? Summary { get; set; }
    public string? CurrentLocation { get; set; }
    public string? PreferredLocation { get; set; }
    public string? LinkedInUrl { get; set; }
    public bool OpenToWork { get; set; }
    public decimal? DesiredSalaryFrom { get; set; }
    public decimal? DesiredSalaryTo { get; set; }
    public CurrencyCode? DesiredSalaryCurrency { get; set; }
    public StoredFileDto? ResumeFile { get; set; }
}
