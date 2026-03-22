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
}
