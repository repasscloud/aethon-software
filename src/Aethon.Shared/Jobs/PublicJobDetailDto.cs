using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;

namespace Aethon.Shared.Jobs;

public sealed class PublicJobDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? Department { get; set; }
    public string? LocationText { get; set; }
    public WorkplaceType WorkplaceType { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public string Description { get; set; } = "";
    public string? Requirements { get; set; }
    public string? Benefits { get; set; }
    public decimal? SalaryFrom { get; set; }
    public decimal? SalaryTo { get; set; }
    public CurrencyCode? SalaryCurrency { get; set; }
    public DateTime? PublishedUtc { get; set; }
    public PublicOrganisationProfileDto Organisation { get; set; } = new();
}
