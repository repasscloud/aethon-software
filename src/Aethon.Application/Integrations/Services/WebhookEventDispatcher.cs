using System.Text.Json;
using Aethon.Application.Abstractions.Integrations;
using Aethon.Data;
using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Integrations.Services;

public sealed class WebhookEventDispatcher : IWebhookEventDispatcher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AethonDbContext _dbContext;

    public WebhookEventDispatcher(AethonDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task QueueAsync(
        Guid organisationId,
        string eventType,
        object payload,
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await _dbContext.WebhookSubscriptions
            .AsNoTracking()
            .Where(x =>
                x.OrganisationId == organisationId &&
                x.IsActive)
            .ToListAsync(cancellationToken);

        if (subscriptions.Count == 0)
        {
            return;
        }

        var payloadJson = JsonSerializer.Serialize(payload, JsonOptions);

        foreach (var subscription in subscriptions)
        {
            if (!SubscriptionMatchesEvent(subscription, eventType))
            {
                continue;
            }

            _dbContext.WebhookDeliveries.Add(new WebhookDelivery
            {
                Id = Guid.NewGuid(),
                WebhookSubscriptionId = subscription.Id,
                EventType = eventType,
                PayloadJson = payloadJson,
                Status = "Pending",
                AttemptCount = 0,
                CreatedUtc = DateTime.UtcNow
            });
        }
    }

    private static bool SubscriptionMatchesEvent(
        WebhookSubscription subscription,
        string eventType)
    {
        if (string.IsNullOrWhiteSpace(subscription.EventsCsv))
        {
            return false;
        }

        var events = subscription.EventsCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return events.Contains(eventType, StringComparer.OrdinalIgnoreCase);
    }
}
