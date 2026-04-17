using FluentValidation;

namespace Aethon.Application.Files.Commands.UploadStoredFile;

public sealed class UploadStoredFileValidator : AbstractValidator<UploadStoredFileCommand>
{
    public UploadStoredFileValidator()
    {
        RuleFor(x => x.OriginalFileName)
            .NotEmpty()
            .MaximumLength(260);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Content)
            .NotEmpty();

        RuleFor(x => x.Content.Length)
            .LessThanOrEqualTo(10 * 1024 * 1024)
            .WithMessage("Files must be 10 MB or smaller.");
    }
}
