using Aethon.Shared.Enums;

namespace Aethon.Shared.Jobs;

public sealed class JobDetailDto
{
    public Guid Id { get; set; }

    public Guid OwnedByOrganisationId { get; set; }
    public string OwnedByOrganisationName { get; set; } = string.Empty;

    public Guid? ManagedByOrganisationId { get; set; }
    public string? ManagedByOrganisationName { get; set; }

    public Guid? ManagedByUserId { get; set; }
    public Guid? OrganisationRecruitmentPartnershipId { get; set; }

    public JobCreatedByType CreatedByType { get; set; }
    public JobStatus Status { get; set; }
    public string? StatusReason { get; set; }
    public JobVisibility Visibility { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? ReferenceCode { get; set; }
    public string? ExternalReference { get; set; }
    public string? Department { get; set; }
    public string? LocationText { get; set; }
    public string? LocationCity { get; set; }
    public string? LocationState { get; set; }
    public string? LocationCountry { get; set; }
    public string? LocationCountryCode { get; set; }
    public double? LocationLatitude { get; set; }
    public double? LocationLongitude { get; set; }
    public string? LocationPlaceId { get; set; }

    public WorkplaceType WorkplaceType { get; set; }
    public EmploymentType EmploymentType { get; set; }

    public string Description { get; set; } = string.Empty;
    public string? Requirements { get; set; }
    public string? Benefits { get; set; }
    public string? Summary { get; set; }

    public decimal? SalaryFrom { get; set; }
    public decimal? SalaryTo { get; set; }
    public CurrencyCode? SalaryCurrency { get; set; }

    public DateTime? PublishedUtc { get; set; }
    public DateTime? ApplyByUtc { get; set; }
    public DateTime? ClosedUtc { get; set; }
    public DateTime? SubmittedForApprovalUtc { get; set; }

    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedUtc { get; set; }

    public string? ExternalApplicationUrl { get; set; }
    public string? ApplicationEmail { get; set; }

    public bool CreatedForUnclaimedCompany { get; set; }

    public JobCategory? Category { get; set; }
    public List<JobRegion> Regions { get; set; } = [];
    public List<string> Countries { get; set; } = [];
    public DateTime? PostingExpiresUtc { get; set; }

    public bool IncludeCompanyLogo { get; set; }
    public bool IsHighlighted { get; set; }
    public DateTime? StickyUntilUtc { get; set; }
    public bool AllowAutoMatch { get; set; }

    public List<string> BenefitsTags { get; set; } = [];
    public string? ApplicationSpecialRequirements { get; set; }
    public string? Keywords { get; set; }
    public string? PoNumber { get; set; }
    public string? ShortUrlCode { get; set; }

    public bool HasCommission { get; set; }
    public decimal? OteFrom { get; set; }
    public decimal? OteTo { get; set; }
    public bool IsImmediateStart { get; set; }
    public string? VideoYouTubeId { get; set; }
    public string? VideoVimeoId { get; set; }
    public string? ScreeningQuestionsJson { get; set; }

    public DateTime CreatedUtc { get; set; }
    public Guid CreatedByIdentityUserId { get; set; }

    public int ApplicationCount { get; set; }
}
