namespace Aethon.Data.Entities;

public sealed class WebhookDelivery
{
    public Guid Id { get; set; }

    public Guid WebhookSubscriptionId { get; set; }
    public WebhookSubscription WebhookSubscription { get; set; } = null!;

    public string EventType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? LastAttemptUtc { get; set; }
    public string? LastError { get; set; }
}
