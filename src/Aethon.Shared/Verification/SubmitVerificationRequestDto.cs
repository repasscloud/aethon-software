namespace Aethon.Shared.Verification;

public sealed class SubmitVerificationRequestDto
{
    public string FullName { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? AdditionalNotes { get; set; }
}
