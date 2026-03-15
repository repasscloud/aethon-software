namespace Aethon.Shared.Auth;

public sealed class RegisterResultDto
{
    public bool Succeeded { get; set; }
    public bool RequiresEmailConfirmation { get; set; }
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string RegistrationType { get; set; } = "";
}
