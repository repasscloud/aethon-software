using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

public sealed class OrganisationJobCredit : EntityBase
{
    public Guid OrganisationId { get; set; }
    public Organisation Organisation { get; set; } = null!;

    public CreditType CreditType { get; set; }
    public CreditSource Source { get; set; }

    public int QuantityOriginal { get; set; }
    public int QuantityRemaining { get; set; }

    public DateTime? ExpiresAt { get; set; }
    public DateTime? ConvertedAt { get; set; }

    // Set when credit came from a Stripe purchase
    public Guid? StripePaymentEventId { get; set; }
    public StripePaymentEvent? StripePaymentEvent { get; set; }

    // Set when credit was manually granted by an admin
    public Guid? GrantedByUserId { get; set; }
    public string? GrantNote { get; set; }

    public ICollection<CreditConsumptionLog> ConsumptionLogs { get; set; } = [];
}
