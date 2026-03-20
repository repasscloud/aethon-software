namespace Aethon.Shared.Integrations;

public sealed class WebhookSubscriptionDto
{
    public Guid Id { get; set; }
    public Guid OrganisationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EndpointUrl { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public IReadOnlyList<string> Events { get; set; } = [];
}
