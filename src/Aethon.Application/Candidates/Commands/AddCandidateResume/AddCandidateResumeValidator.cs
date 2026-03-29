using FluentValidation;

namespace Aethon.Application.Candidates.Commands.AddCandidateResume;

public sealed class AddCandidateResumeValidator : AbstractValidator<AddCandidateResumeCommand>
{
    public AddCandidateResumeValidator()
    {
        RuleFor(x => x.StoredFileId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(1000);
    }
}
