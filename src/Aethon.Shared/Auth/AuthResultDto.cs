namespace Aethon.Shared.Auth;

public sealed class AuthResultDto
{
    public bool IsAuthenticated { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? TenantId { get; set; }
    public string? TenantSlug { get; set; }
    public string? AppType { get; set; }
    public string? OrganisationId { get; set; }
    public string? OrganisationName { get; set; }
    public string? OrganisationType { get; set; }
    public bool IsOrganisationOwner { get; set; }
    public string? CompanyRole { get; set; }
    public string? RecruiterRole { get; set; }
    public bool HasJobSeekerProfile { get; set; }
    public List<string> Roles { get; set; } = [];
}
