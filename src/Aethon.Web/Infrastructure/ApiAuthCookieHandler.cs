using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Aethon.Web.Infrastructure;

public sealed class ApiAuthCookieHandler : DelegatingHandler
{
    private const string AuthCookieName = "Aethon.Auth";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiAuthCookieHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.Request.Cookies.TryGetValue(AuthCookieName, out var authCookie) == true &&
            !string.IsNullOrWhiteSpace(authCookie))
        {
            request.Headers.Remove("Cookie");
            request.Headers.Add("Cookie", $"{AuthCookieName}={authCookie}");
        }

        if (request.Headers.Accept.Count == 0)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        return base.SendAsync(request, cancellationToken);
    }
}
