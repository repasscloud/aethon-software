using System.Security.Claims;
using Aethon.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Aethon.Api.Auth;

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsStaff
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user is not null && (
                user.IsInRole("SuperAdmin") ||
                user.IsInRole("Admin") ||
                user.IsInRole("Support"));
        }
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null)
            {
                return Guid.Empty;
            }

            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
