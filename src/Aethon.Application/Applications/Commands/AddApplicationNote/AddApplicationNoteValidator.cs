using FluentValidation;

namespace Aethon.Application.Applications.Commands.AddApplicationNote;

public sealed class AddApplicationNoteValidator : AbstractValidator<AddApplicationNoteCommand>
{
    public AddApplicationNoteValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(4000);
    }
}
