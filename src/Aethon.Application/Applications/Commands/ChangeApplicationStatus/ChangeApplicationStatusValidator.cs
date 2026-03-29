using FluentValidation;

namespace Aethon.Application.Applications.Commands.ChangeApplicationStatus;

public sealed class ChangeApplicationStatusValidator : AbstractValidator<ChangeApplicationStatusCommand>
{
    public ChangeApplicationStatusValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Reason)
            .MaximumLength(500);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);
    }
}
