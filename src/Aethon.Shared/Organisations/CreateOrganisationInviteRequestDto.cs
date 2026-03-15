using System.ComponentModel.DataAnnotations;

namespace Aethon.Shared.Organisations;

public sealed class CreateOrganisationInviteRequestDto : IValidatableObject
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; set; } = "";

    [MaxLength(50)]
    public string? CompanyRole { get; set; }

    [MaxLength(50)]
    public string? RecruiterRole { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var hasCompanyRole = !string.IsNullOrWhiteSpace(CompanyRole);
        var hasRecruiterRole = !string.IsNullOrWhiteSpace(RecruiterRole);

        if (hasCompanyRole == hasRecruiterRole)
        {
            yield return new ValidationResult(
                "Exactly one role type must be provided.",
                [nameof(CompanyRole), nameof(RecruiterRole)]);
        }
    }
}
