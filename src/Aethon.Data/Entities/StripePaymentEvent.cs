using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

public sealed class StripePaymentEvent : EntityBase
{
    public string StripeEventId { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public long? AmountTotal { get; set; }
    public string? Currency { get; set; }
    public string? CustomerEmail { get; set; }
    public string PayloadJson { get; set; } = null!;
    public StripeEventStatus Status { get; set; } = StripeEventStatus.Pending;
    public string? InternalNotes { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public DateTime? CompletedUtc { get; set; }

    // Resolved from webhook metadata
    public Guid? OrganisationId { get; set; }
    public Organisation? Organisation { get; set; }

    public string? PurchaseType { get; set; }      // "verification", "job_credits", "sticky", "addon"
    public string? ProductId { get; set; }          // Stripe Product ID
    public string? PriceId { get; set; }            // Stripe Price ID
    public string? PurchaseMetaJson { get; set; }   // Full metadata dict as JSON
}
