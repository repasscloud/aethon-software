using Aethon.Shared.Enums;

namespace Aethon.Shared.Billing;

public sealed class CreditBalanceItemDto
{
    public Guid Id { get; set; }
    public CreditType CreditType { get; set; }
    public CreditSource Source { get; set; }
    public int QuantityOriginal { get; set; }
    public int QuantityRemaining { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public DateTime CreatedUtc { get; set; }
}
