using Aethon.Shared.Enums;

namespace Aethon.Shared.Jobs;

public sealed class JobDetailDto
{
    public string Id { get; set; } = "";
    public string OrganisationId { get; set; } = "";
    public string OrganisationName { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Department { get; set; }
    public string? LocationText { get; set; }
    public WorkplaceType WorkplaceType { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public JobStatus Status { get; set; }
    public string Description { get; set; } = "";
    public string? Requirements { get; set; }
    public string? Benefits { get; set; }
    public decimal? SalaryFrom { get; set; }
    public decimal? SalaryTo { get; set; }
    public CurrencyCode? SalaryCurrency { get; set; }
    public string CreatedByUserId { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
    public DateTime? PublishedUtc { get; set; }
    public DateTime? ClosedUtc { get; set; }
}
