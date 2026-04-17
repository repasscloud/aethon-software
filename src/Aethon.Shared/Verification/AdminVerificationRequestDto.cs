namespace Aethon.Shared.Verification;

public sealed class AdminVerificationRequestDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserDisplayName { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? AdditionalNotes { get; set; }
    public string Status { get; set; } = null!;
    public DateTime RequestedUtc { get; set; }
    public DateTime? ReviewedUtc { get; set; }
    public string? ReviewNotes { get; set; }
    public string? ReviewerType { get; set; }
}
