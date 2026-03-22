using Aethon.Shared.Enums;

namespace Aethon.Application.Jobs.Commands.CreateJob;

public sealed class CreateJobCommand
{
    public Guid OwnedByOrganisationId { get; init; }
    public Guid? ManagedByOrganisationId { get; init; }
    public Guid? ManagedByUserId { get; init; }
    public Guid? OrganisationRecruitmentPartnershipId { get; init; }
    public JobCreatedByType CreatedByType { get; init; } = JobCreatedByType.CompanyUser;
    public JobVisibility Visibility { get; init; } = JobVisibility.Public;
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

    public JobCategory? Category { get; init; }
    public List<JobRegion> Regions { get; init; } = [];
    public List<string> Countries { get; init; } = [];
    public DateTime? PostingExpiresUtc { get; init; }
    public bool IncludeCompanyLogo { get; init; }
    public bool IsHighlighted { get; init; }
    public DateTime? StickyUntilUtc { get; init; }
    public bool AllowAutoMatch { get; init; }
    public List<string> BenefitsTags { get; init; } = [];
    public string? ApplicationSpecialRequirements { get; init; }
    public string? Keywords { get; init; }
    public string? PoNumber { get; init; }

    public bool HasCommission { get; init; }
    public decimal? OteFrom { get; init; }
    public decimal? OteTo { get; init; }
    public bool IsImmediateStart { get; init; }
    public string? VideoYouTubeId { get; init; }
    public string? VideoVimeoId { get; init; }
    public JobStatus Status { get; init; } = JobStatus.Draft;
    public string? ScreeningQuestionsJson { get; init; }
}
