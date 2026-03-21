using System.ComponentModel.DataAnnotations;
using Aethon.Shared.Enums;

namespace Aethon.Shared.Jobs;

public sealed class CreateJobRequestDto : IValidatableObject
{
    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = "";

    [MaxLength(150)]
    public string? Department { get; set; }

    [MaxLength(250)]
    public string? LocationText { get; set; }

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

    [Required]
    public decimal? SalaryFrom { get; set; }

    [Required]
    public decimal? SalaryTo { get; set; }

    public CurrencyCode? SalaryCurrency { get; set; }

    [Required]
    public JobStatus? Status { get; set; }

    public string? ExternalApplicationUrl { get; set; }
    public string? ApplicationEmail { get; set; }

    public JobVisibility Visibility { get; set; } = JobVisibility.Public;
    public JobCategory? Category { get; set; }
    public List<JobRegion> Regions { get; set; } = [];
    public List<string> Countries { get; set; } = [];
    public DateTime? PostingExpiresUtc { get; set; }

    public bool IncludeCompanyLogo { get; set; }
    public bool IsHighlighted { get; set; }
    public DateTime? StickyUntilUtc { get; set; }
    public bool AllowAutoMatch { get; set; }

    public List<string> BenefitsTags { get; set; } = [];

    [MaxLength(2000)]
    public string? ApplicationSpecialRequirements { get; set; }

    [MaxLength(500)]
    public string? Keywords { get; set; }

    [MaxLength(100)]
    public string? PoNumber { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SalaryFrom.HasValue && SalaryFrom.Value < 0)
        {
            yield return new ValidationResult("Salary from must be zero or greater.", [nameof(SalaryFrom)]);
        }

        if (SalaryTo.HasValue && SalaryTo.Value < 0)
        {
            yield return new ValidationResult("Salary to must be zero or greater.", [nameof(SalaryTo)]);
        }

        if (SalaryFrom.HasValue && SalaryTo.HasValue && SalaryTo.Value < SalaryFrom.Value)
        {
            yield return new ValidationResult("Salary to must be greater than or equal to salary from.", [nameof(SalaryTo)]);
        }

        if ((SalaryFrom.HasValue || SalaryTo.HasValue) && SalaryCurrency is null)
        {
            yield return new ValidationResult("Salary currency is required when salary is provided.", [nameof(SalaryCurrency)]);
        }
    }
}
