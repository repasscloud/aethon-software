using Aethon.Data.Identity;
using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

public class JobSeekerProfile : EntityBase
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string? Headline { get; set; }
    public string? Summary { get; set; }
    public string? CurrentLocation { get; set; }
    public string? PreferredLocation { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? ResumeFileId { get; set; }

    public bool OpenToWork { get; set; }

    public decimal? DesiredSalaryFrom { get; set; }
    public decimal? DesiredSalaryTo { get; set; }
    public CurrencyCode? DesiredSalaryCurrency { get; set; }
}