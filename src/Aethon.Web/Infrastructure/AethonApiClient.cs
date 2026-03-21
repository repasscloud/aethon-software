using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Authorization;

namespace Aethon.Web.Infrastructure;

/// <summary>
/// Scoped HTTP client factory for the Aethon API.
/// Works correctly in both Blazor Server prerendering (HttpContext available)
/// and the SignalR interactive phase (HttpContext is null) by reading the
/// JWT from <see cref="AuthenticationStateProvider"/> instead of IHttpContextAccessor.
/// </summary>
public sealed class AethonApiClient
{
    /// <summary>
    /// JSON options matching the API's serialiser — enums as strings, case-insensitive.
    /// Use this for every GetFromJsonAsync / ReadFromJsonAsync call so that enum values
    /// like "Draft", "Pending", "Verified" round-trip correctly.
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true) }
    };

    private readonly IHttpClientFactory _factory;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AethonApiClient(IHttpClientFactory factory, AuthenticationStateProvider authStateProvider)
    {
        _factory = factory;
        _authStateProvider = authStateProvider;
    }

    /// <summary>
    /// Returns a new <see cref="HttpClient"/> pointed at the Aethon API, with the
    /// current user's Bearer token attached when the user is authenticated.
    /// </summary>
    public async Task<HttpClient> CreateAsync()
    {
        var client = _factory.CreateClient("AethonApiBase");

        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var token = authState.User.FindFirstValue("access_token");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }
}
