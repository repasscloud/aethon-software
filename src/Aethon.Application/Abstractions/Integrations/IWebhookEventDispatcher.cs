namespace Aethon.Application.Abstractions.Integrations;

public interface IWebhookEventDispatcher
{
    Task QueueAsync(
        Guid organisationId,
        string eventType,
        object payload,
        CancellationToken cancellationToken = default);
}
