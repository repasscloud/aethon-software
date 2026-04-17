using Aethon.Shared.Enums;
using FluentValidation;

namespace Aethon.Application.Candidates.Commands.UpsertMyCandidateProfile;

public sealed class UpsertMyCandidateProfileValidator : AbstractValidator<UpsertMyCandidateProfileCommand>
{
    public UpsertMyCandidateProfileValidator()
    {
        RuleFor(x => x.FirstName).MaximumLength(100);
        RuleFor(x => x.MiddleName).MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100);

        RuleFor(x => x.PhoneNumber).MaximumLength(50);
        RuleFor(x => x.WhatsAppNumber).MaximumLength(50);

        RuleFor(x => x.Headline).MaximumLength(200);
        RuleFor(x => x.Summary).MaximumLength(4000);
        RuleFor(x => x.AboutMe).MaximumLength(1000);

        RuleFor(x => x.CurrentLocation).MaximumLength(250);
        RuleFor(x => x.PreferredLocation).MaximumLength(250);
        RuleFor(x => x.AvailabilityText).MaximumLength(250);

        RuleFor(x => x.LinkedInUrl).MaximumLength(500);

        RuleFor(x => x.Slug).MaximumLength(200);

        // School leaver: birth month + year required
        When(x => x.AgeGroup == ApplicantAgeGroup.SchoolLeaver, () =>
        {
            RuleFor(x => x.BirthMonth)
                .NotNull().WithMessage("Birth month is required for school leaver accounts.")
                .InclusiveBetween(1, 12).When(x => x.BirthMonth.HasValue);

            RuleFor(x => x.BirthYear)
                .NotNull().WithMessage("Birth year is required for school leaver accounts.")
                .InclusiveBetween(1900, 9999).When(x => x.BirthYear.HasValue);
        });

        // Adult: no birth date should be provided
        When(x => x.AgeGroup == ApplicantAgeGroup.Adult, () =>
        {
            RuleFor(x => x.BirthMonth)
                .Null().WithMessage("Birth month must not be provided for adult accounts.");

            RuleFor(x => x.BirthYear)
                .Null().WithMessage("Birth year must not be provided for adult accounts.");
        });
    }
}
