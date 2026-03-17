using System.Security.Claims;

namespace Aethon.Api.Common;

public static class UserExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (value is null)
            throw new InvalidOperationException("User id claim is missing.");

        return Guid.Parse(value);
    }
}