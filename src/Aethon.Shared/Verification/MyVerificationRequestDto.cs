namespace Aethon.Shared.Verification;

public sealed class MyVerificationRequestDto
{
    public Guid Id { get; set; }

    /// <summary>Pending | Approved | Denied</summary>
    public string Status { get; set; } = null!;

    public DateTime RequestedUtc { get; set; }
    public DateTime? ReviewedUtc { get; set; }
    public string? ReviewNotes { get; set; }
}
