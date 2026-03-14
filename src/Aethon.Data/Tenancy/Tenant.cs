namespace Aethon.Data.Tenancy;

public sealed class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public bool IsEnabled { get; set; } = true;
}
