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
    }
}
