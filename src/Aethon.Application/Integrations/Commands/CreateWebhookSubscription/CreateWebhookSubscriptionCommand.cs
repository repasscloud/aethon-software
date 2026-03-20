namespace Aethon.Application.Integrations.Commands.CreateWebhookSubscription;

public sealed class CreateWebhookSubscriptionCommand
{
    public Guid OrganisationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string EndpointUrl { get; init; } = string.Empty;
    public string Secret { get; init; } = string.Empty;
    public IReadOnlyList<string> Events { get; init; } = [];
}
