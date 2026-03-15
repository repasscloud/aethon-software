using Aethon.Shared.Enums;

namespace Aethon.Shared.Jobs;

public sealed class PublicJobListItemDto
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string OrganisationName { get; set; } = "";
    public string? OrganisationSlug { get; set; }
    public string? OrganisationLogoUrl { get; set; }
    public string? Department { get; set; }
    public string? LocationText { get; set; }
    public WorkplaceType WorkplaceType { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public decimal? SalaryFrom { get; set; }
    public decimal? SalaryTo { get; set; }
    public CurrencyCode? SalaryCurrency { get; set; }
    public DateTime? PublishedUtc { get; set; }
}
