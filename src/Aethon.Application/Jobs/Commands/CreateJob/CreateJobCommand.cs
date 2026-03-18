using Aethon.Shared.Enums;

namespace Aethon.Application.Jobs.Commands.CreateJob;

public sealed class CreateJobCommand
{
    public Guid OwnedByOrganisationId { get; init; }
    public Guid? ManagedByOrganisationId { get; init; }
    public Guid? ManagedByUserId { get; init; }
    public Guid? OrganisationRecruitmentPartnershipId { get; init; }
    public JobCreatedByType CreatedByType { get; init; } = JobCreatedByType.CompanyUser;
    public JobVisibility Visibility { get; init; } = JobVisibility.Private;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public string? Department { get; init; }
    public string? LocationText { get; init; }
    public WorkplaceType WorkplaceType { get; init; }
    public EmploymentType EmploymentType { get; init; }
    public string? Requirements { get; init; }
    public string? Benefits { get; init; }
    public string? ReferenceCode { get; init; }
    public string? ExternalReference { get; init; }
    public decimal? SalaryFrom { get; init; }
    public decimal? SalaryTo { get; init; }
    public CurrencyCode? SalaryCurrency { get; init; }
    public DateTime? ApplyByUtc { get; init; }
    public string? ExternalApplicationUrl { get; init; }
    public string? ApplicationEmail { get; init; }
    public bool CreatedForUnclaimedCompany { get; init; }
}
