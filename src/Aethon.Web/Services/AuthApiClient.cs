using System.Net.Http.Json;
using Aethon.Shared.Auth;

namespace Aethon.Web.Services;

public sealed class AuthApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HttpResponseMessage> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("AethonApi");

        // Cookie-based login for browser
        return await client.PostAsJsonAsync("/login?useCookies=true", request, cancellationToken);
    }

    public async Task<AuthResultDto?> MeAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("AethonApi");
        return await client.GetFromJsonAsync<AuthResultDto>("/auth/me", cancellationToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("AethonApi");
        await client.PostAsync("/auth/logout-cookie", null, cancellationToken);
    }
}
