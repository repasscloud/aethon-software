using System.ComponentModel.DataAnnotations;
using Aethon.Shared.Enums;

namespace Aethon.Shared.Jobs;

public sealed class UpdateJobSeekerProfileRequestDto : IValidatableObject
{
    [MaxLength(200)]
    public string? Headline { get; set; }

    [MaxLength(4000)]
    public string? Summary { get; set; }

    [MaxLength(250)]
    public string? CurrentLocation { get; set; }

    [MaxLength(250)]
    public string? PreferredLocation { get; set; }

    [MaxLength(500)]
    public string? LinkedInUrl { get; set; }

    public bool OpenToWork { get; set; }

    public decimal? DesiredSalaryFrom { get; set; }
    public decimal? DesiredSalaryTo { get; set; }
    public CurrencyCode? DesiredSalaryCurrency { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DesiredSalaryFrom.HasValue && DesiredSalaryFrom.Value < 0)
        {
            yield return new ValidationResult("Desired salary from must be zero or greater.", [nameof(DesiredSalaryFrom)]);
        }

        if (DesiredSalaryTo.HasValue && DesiredSalaryTo.Value < 0)
        {
            yield return new ValidationResult("Desired salary to must be zero or greater.", [nameof(DesiredSalaryTo)]);
        }

        if (DesiredSalaryFrom.HasValue && DesiredSalaryTo.HasValue && DesiredSalaryTo.Value < DesiredSalaryFrom.Value)
        {
            yield return new ValidationResult("Desired salary to must be greater than or equal to desired salary from.", [nameof(DesiredSalaryTo)]);
        }

        if ((DesiredSalaryFrom.HasValue || DesiredSalaryTo.HasValue) && DesiredSalaryCurrency is null)
        {
            yield return new ValidationResult("Desired salary currency is required when salary is provided.", [nameof(DesiredSalaryCurrency)]);
        }
    }
}
