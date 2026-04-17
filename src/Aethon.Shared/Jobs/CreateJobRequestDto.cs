using System.ComponentModel.DataAnnotations;
using Aethon.Shared.Enums;

namespace Aethon.Shared.Jobs;

public sealed class CreateJobRequestDto : IValidatableObject
{
    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = "";

    [Required]
    [MinLength(50)]
    [MaxLength(300)]
    public string Summary { get; set; } = "";

    [MaxLength(150)]
    public string? Department { get; set; }

    [Required]
    [MaxLength(250)]
    public string LocationText { get; set; } = "";

    [MaxLength(150)]
    public string? LocationCity { get; set; }
    [MaxLength(150)]
    public string? LocationState { get; set; }
    [MaxLength(100)]
    public string? LocationCountry { get; set; }
    [MaxLength(10)]
    public string? LocationCountryCode { get; set; }
    public double? LocationLatitude { get; set; }
    public double? LocationLongitude { get; set; }
    [MaxLength(500)]
    public string? LocationPlaceId { get; set; }

    [Required]
    public WorkplaceType? WorkplaceType { get; set; }

    [Required]
    public EmploymentType? EmploymentType { get; set; }

    [Required]
    [MaxLength(20000)]
    public string Description { get; set; } = "";

    [MaxLength(20000)]
    public string? Requirements { get; set; }

    [MaxLength(20000)]
    public string? Benefits { get; set; }

    public decimal? SalaryFrom { get; set; }
    public decimal? SalaryTo { get; set; }
    public CurrencyCode? SalaryCurrency { get; set; }

    public bool HasCommission { get; set; }
    public decimal? OteFrom { get; set; }
    public decimal? OteTo { get; set; }

    public bool IsImmediateStart { get; set; }

    [MaxLength(20)]
    public string? VideoYouTubeId { get; set; }

    [MaxLength(20)]
    public string? VideoVimeoId { get; set; }

    [Required]
    public JobStatus? Status { get; set; }

    public string? ExternalApplicationUrl { get; set; }
    public string? ApplicationEmail { get; set; }

    public JobVisibility Visibility { get; set; } = JobVisibility.Public;
    [Required] public JobCategory? Category { get; set; }
    public List<JobRegion> Regions { get; set; } = [];
    public List<string> Countries { get; set; } = [];
    public DateTime? PostingExpiresUtc { get; set; }

    public JobPostingTier PostingTier { get; set; } = JobPostingTier.Standard;

    public bool IncludeCompanyLogo { get; set; }
    public bool IsHighlighted { get; set; }

    [MaxLength(20)]
    public string? HighlightColour { get; set; }

    public bool HasAiCandidateMatching { get; set; }
    public DateTime? StickyUntilUtc { get; set; }
    public bool AllowAutoMatch { get; set; }

    public List<string> BenefitsTags { get; set; } = [];

    [MaxLength(20000)]
    public string? ApplicationSpecialRequirements { get; set; }

    [MaxLength(500)]
    public string? Keywords { get; set; }

    [MaxLength(100)]
    public string? PoNumber { get; set; }

    /// <summary>JSON-serialised ScreeningConfig for this job.</summary>
    public string? ScreeningQuestionsJson { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SalaryFrom.HasValue && SalaryFrom.Value < 0)
            yield return new ValidationResult("Salary from must be zero or greater.", [nameof(SalaryFrom)]);

        if (SalaryTo.HasValue && SalaryTo.Value < 0)
            yield return new ValidationResult("Salary to must be zero or greater.", [nameof(SalaryTo)]);

        if (SalaryFrom.HasValue && SalaryTo.HasValue && SalaryTo.Value < SalaryFrom.Value)
            yield return new ValidationResult("Salary to must be greater than or equal to salary from.", [nameof(SalaryTo)]);

        if ((SalaryFrom.HasValue || SalaryTo.HasValue) && SalaryCurrency is null)
            yield return new ValidationResult("Salary currency is required when salary is provided.", [nameof(SalaryCurrency)]);

        if (HasCommission && OteCurrency is null && SalaryCurrency is null)
            yield return new ValidationResult("A currency is required when commission/OTE is specified.", [nameof(SalaryCurrency)]);

        if (!string.IsNullOrWhiteSpace(VideoYouTubeId) && !string.IsNullOrWhiteSpace(VideoVimeoId))
            yield return new ValidationResult("Only one video source (YouTube or Vimeo) can be specified.", [nameof(VideoYouTubeId)]);
    }

    // Helper — not a real property, just for validation message context
    private CurrencyCode? OteCurrency => SalaryCurrency;
}
