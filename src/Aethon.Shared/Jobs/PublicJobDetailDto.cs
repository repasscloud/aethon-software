using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;

namespace Aethon.Shared.Jobs;

public sealed class PublicJobDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? Summary { get; set; }
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
    public bool HasCommission { get; set; }
    public decimal? OteFrom { get; set; }
    public decimal? OteTo { get; set; }
    public DateTime? PublishedUtc { get; set; }
    public JobCategory? Category { get; set; }
    public List<JobRegion> Regions { get; set; } = [];
    public List<string> BenefitsTags { get; set; } = [];
    public string? ApplicationSpecialRequirements { get; set; }
    public string? ExternalApplicationUrl { get; set; }
    public string? ApplicationEmail { get; set; }
    public bool IsImmediateStart { get; set; }
    public string? VideoYouTubeId { get; set; }
    public string? VideoVimeoId { get; set; }
    /// <summary>JSON-serialised ScreeningConfig — used by the apply page to render questions.</summary>
    public string? ScreeningQuestionsJson { get; set; }
    public PublicOrganisationProfileDto Organisation { get; set; } = new();
}
