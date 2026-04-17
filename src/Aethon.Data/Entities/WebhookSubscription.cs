namespace Aethon.Data.Entities;

public sealed class WebhookSubscription
{
    public Guid Id { get; set; }

    public Guid OrganisationId { get; set; }
    public Organisation Organisation { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string EndpointUrl { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public string EventsCsv { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public DateTime CreatedUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
}
