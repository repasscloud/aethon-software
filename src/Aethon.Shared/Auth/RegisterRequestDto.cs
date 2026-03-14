namespace Aethon.Shared.Auth;

public sealed class RegisterRequestDto
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string DisplayName { get; set; } = "";
}
