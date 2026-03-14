using Microsoft.AspNetCore.Identity;

namespace Aethon.Data.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = "";
    public bool IsEnabled { get; set; } = true;
}
