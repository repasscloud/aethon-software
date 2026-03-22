using System.ComponentModel.DataAnnotations;
using Aethon.Shared.Enums;

namespace Aethon.Shared.Candidates;

public sealed class UpdateMyCandidateProfileRequestDto : IValidatableObject
{
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? MiddleName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    [MaxLength(50)]
    public string? WhatsAppNumber { get; set; }

    [MaxLength(200)]
    public string? Headline { get; set; }

    [MaxLength(4000)]
    public string? Summary { get; set; }

    [MaxLength(1000)]
    public string? AboutMe { get; set; }

    [MaxLength(250)]
    public string? CurrentLocation { get; set; }

    [MaxLength(250)]
    public string? PreferredLocation { get; set; }

    [MaxLength(250)]
    public string? AvailabilityText { get; set; }

    [MaxLength(500)]
    public string? LinkedInUrl { get; set; }

    public bool OpenToWork { get; set; }

    public decimal? DesiredSalaryFrom { get; set; }
    public decimal? DesiredSalaryTo { get; set; }
    public CurrencyCode? DesiredSalaryCurrency { get; set; }

    public bool? WillRelocate { get; set; }
    public bool? RequiresSponsorship { get; set; }
    public bool? HasWorkRights { get; set; }

    public bool IsPublicProfileEnabled { get; set; }
    public bool IsSearchable { get; set; }

    [MaxLength(200)]
    public string? Slug { get; set; }

    public ProfileVisibility ProfileVisibility { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DesiredSalaryFrom.HasValue && DesiredSalaryFrom.Value < 0)
        {
            yield return new ValidationResult(
                "Desired salary from must be zero or greater.",
                [nameof(DesiredSalaryFrom)]);
        }

        if (DesiredSalaryTo.HasValue && DesiredSalaryTo.Value < 0)
        {
            yield return new ValidationResult(
                "Desired salary to must be zero or greater.",
                [nameof(DesiredSalaryTo)]);
        }

        if (DesiredSalaryFrom.HasValue &&
            DesiredSalaryTo.HasValue &&
            DesiredSalaryTo.Value < DesiredSalaryFrom.Value)
        {
            yield return new ValidationResult(
                "Desired salary to must be greater than or equal to desired salary from.",
                [nameof(DesiredSalaryTo)]);
        }

        if ((DesiredSalaryFrom.HasValue || DesiredSalaryTo.HasValue) &&
            DesiredSalaryCurrency is null)
        {
            yield return new ValidationResult(
                "Desired salary currency is required when salary is provided.",
                [nameof(DesiredSalaryCurrency)]);
        }
    }
}
