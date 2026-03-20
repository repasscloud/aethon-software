using FluentValidation;

namespace Aethon.Application.Applications.Commands.SubmitJobApplication;

public sealed class SubmitJobApplicationValidator : AbstractValidator<SubmitJobApplicationCommand>
{
    public SubmitJobApplicationValidator()
    {
        RuleFor(x => x.JobId)
            .NotEmpty();

        RuleFor(x => x.CoverLetter)
            .MaximumLength(10000);

        RuleFor(x => x.Source)
            .MaximumLength(200);
    }
}
