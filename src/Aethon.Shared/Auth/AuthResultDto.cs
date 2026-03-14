namespace Aethon.Shared.Auth;

public sealed class AuthResultDto
{
    public bool IsAuthenticated { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? TenantId { get; set; }
    public string? TenantSlug { get; set; }
    public List<string> Roles { get; set; } = [];
}
