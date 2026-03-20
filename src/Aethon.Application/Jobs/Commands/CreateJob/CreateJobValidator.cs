using FluentValidation;

namespace Aethon.Application.Jobs.Commands.CreateJob;

public sealed class CreateJobValidator : AbstractValidator<CreateJobCommand>
{
    public CreateJobValidator()
    {
        RuleFor(x => x.OwnedByOrganisationId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(20000);

        RuleFor(x => x.Summary)
            .MaximumLength(4000);

        RuleFor(x => x.Department)
            .MaximumLength(200);

        RuleFor(x => x.LocationText)
            .MaximumLength(250);

        RuleFor(x => x.Requirements)
            .MaximumLength(20000);

        RuleFor(x => x.Benefits)
            .MaximumLength(20000);

        RuleFor(x => x.ReferenceCode)
            .MaximumLength(100);

        RuleFor(x => x.ExternalReference)
            .MaximumLength(100);

        RuleFor(x => x.ExternalApplicationUrl)
            .MaximumLength(1000);

        RuleFor(x => x.ApplicationEmail)
            .MaximumLength(320)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.ApplicationEmail));

        RuleFor(x => x.SalaryTo)
            .GreaterThanOrEqualTo(x => x.SalaryFrom!.Value)
            .When(x => x.SalaryFrom.HasValue && x.SalaryTo.HasValue);

        RuleFor(x => x.SalaryCurrency)
            .NotNull()
            .When(x => x.SalaryFrom.HasValue || x.SalaryTo.HasValue);
    }
}
