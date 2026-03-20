using FluentValidation;

namespace Aethon.Application.Integrations.Commands.CreateWebhookSubscription;

public sealed class CreateWebhookSubscriptionValidator : AbstractValidator<CreateWebhookSubscriptionCommand>
{
    public CreateWebhookSubscriptionValidator()
    {
        RuleFor(x => x.OrganisationId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.EndpointUrl)
            .NotEmpty()
            .MaximumLength(2000)
            .Must(x => Uri.TryCreate(x, UriKind.Absolute, out _))
            .WithMessage("EndpointUrl must be a valid absolute URL.");

        RuleFor(x => x.Secret)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Events)
            .NotEmpty();

        RuleForEach(x => x.Events)
            .NotEmpty()
            .MaximumLength(100);
    }
}
