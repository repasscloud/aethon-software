namespace Aethon.Data.Tenancy;

public sealed class UserTenantMembership
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public Identity.ApplicationUser? User { get; set; }
    public Tenant? Tenant { get; set; }
}
