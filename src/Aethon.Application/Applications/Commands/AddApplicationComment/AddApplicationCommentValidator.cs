using FluentValidation;

namespace Aethon.Application.Applications.Commands.AddApplicationComment;

public sealed class AddApplicationCommentValidator : AbstractValidator<AddApplicationCommentCommand>
{
    public AddApplicationCommentValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(4000);
    }
}
