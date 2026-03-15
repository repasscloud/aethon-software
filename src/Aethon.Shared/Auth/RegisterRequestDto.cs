using System.ComponentModel.DataAnnotations;

namespace Aethon.Shared.Auth;

public sealed class RegisterRequestDto : IValidatableObject
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(12)]
    public string Password { get; set; } = "";

    [Required]
    public string ConfirmPassword { get; set; } = "";

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = "";

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = "";

    [Required]
    [MaxLength(50)]
    public string RegistrationType { get; set; } = "";

    [MaxLength(250)]
    public string? OrganisationName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
        {
            yield return new ValidationResult(
                "Password and confirm password do not match.",
                [nameof(ConfirmPassword)]);
        }

        var normalizedType = RegistrationType.Trim().ToLowerInvariant();

        if (normalizedType is not "company" and not "recruiter" and not "jobseeker")
        {
            yield return new ValidationResult(
                "Registration type is invalid.",
                [nameof(RegistrationType)]);
        }

        if ((normalizedType is "company" or "recruiter") &&
            string.IsNullOrWhiteSpace(OrganisationName))
        {
            yield return new ValidationResult(
                "Organisation name is required.",
                [nameof(OrganisationName)]);
        }
    }
}
