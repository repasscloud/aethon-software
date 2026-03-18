using Aethon.Shared.Enums;

namespace Aethon.Application.Jobs.Queries.GetJobById;

public sealed class JobDetailModel
{
    public Guid Id { get; init; }
    public Guid OwnedByOrganisationId { get; init; }
    public Guid? ManagedByOrganisationId { get; init; }
    public Guid? ManagedByUserId { get; init; }
    public Guid? OrganisationRecruitmentPartnershipId { get; init; }
    public JobCreatedByType CreatedByType { get; init; }
    public JobStatus Status { get; init; }
    public string? StatusReason { get; init; }
    public JobVisibility Visibility { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? ReferenceCode { get; init; }
    public string? ExternalReference { get; init; }
    public string? Department { get; init; }
    public string? LocationText { get; init; }
    public WorkplaceType WorkplaceType { get; init; }
    public EmploymentType EmploymentType { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? Requirements { get; init; }
    public string? Benefits { get; init; }
    public string? Summary { get; init; }
    public decimal? SalaryFrom { get; init; }
    public decimal? SalaryTo { get; init; }
    public CurrencyCode? SalaryCurrency { get; init; }
    public DateTime? PublishedUtc { get; init; }
    public DateTime? ApplyByUtc { get; init; }
    public DateTime? ClosedUtc { get; init; }
    public DateTime? SubmittedForApprovalUtc { get; init; }
    public Guid? ApprovedByUserId { get; init; }
    public DateTime? ApprovedUtc { get; init; }
    public string? ExternalApplicationUrl { get; init; }
    public string? ApplicationEmail { get; init; }
    public bool CreatedForUnclaimedCompany { get; init; }
    public DateTime CreatedUtc { get; init; }
    public Guid CreatedByIdentityUserId { get; init; }
    public int ApplicationCount { get; init; }
}
