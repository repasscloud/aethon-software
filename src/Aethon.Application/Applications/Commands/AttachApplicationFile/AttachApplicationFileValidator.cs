using FluentValidation;

namespace Aethon.Application.Applications.Commands.AttachApplicationFile;

public sealed class AttachApplicationFileValidator : AbstractValidator<AttachApplicationFileCommand>
{
    public AttachApplicationFileValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty();
        RuleFor(x => x.StoredFileId).NotEmpty();
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
