using Aethon.Shared.Enums;

namespace Aethon.Shared.Jobs;

public sealed class JobDetailDto
{
    public Guid Id { get; set; }

    public Guid CompanyOrganisationId { get; set; }
    public string? CompanyOrganisationName { get; set; }
    public Guid? ManagedByRecruiterOrganisationId { get; set; }
    public string? ManagedByRecruiterOrganisationName { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Description { get; set; } = string.Empty;

    public string? Department { get; set; }
    public string? Location { get; set; }

    public WorkplaceType WorkplaceType { get; set; }
    public EmploymentType EmploymentType { get; set; }

    public string? Requirements { get; set; }
    public string? Benefits { get; set; }

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public CurrencyCode? SalaryCurrency { get; set; }

    public JobStatus Status { get; set; }
    public string? StatusReason { get; set; }

    public DateTime CreatedUtc { get; set; }
    public DateTime? SubmittedForApprovalUtc { get; set; }
    public DateTime? ApprovedUtc { get; set; }
    public DateTime? PublishedUtc { get; set; }
    public DateTime? ClosedUtc { get; set; }
}