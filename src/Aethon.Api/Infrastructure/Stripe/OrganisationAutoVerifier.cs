using Aethon.Data.Entities;

namespace Aethon.Api.Infrastructure.Stripe;

/// <summary>
/// Runs automated URL reachability checks against an organisation's website
/// and social media presence to determine if it qualifies for Standard Employer
/// Verification without manual admin review.
/// </summary>
public sealed class OrganisationAutoVerifier : IOrganisationAutoVerifier
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OrganisationAutoVerifier> _logger;

    public OrganisationAutoVerifier(IHttpClientFactory httpClientFactory, ILogger<OrganisationAutoVerifier> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<bool> CheckAsync(Organisation org, CancellationToken ct = default)
    {
        // Website must be present and return 2xx
        if (string.IsNullOrWhiteSpace(org.WebsiteUrl))
        {
            _logger.LogInformation("Auto-verify failed for org {OrgId}: WebsiteUrl is empty.", org.Id);
            return false;
        }

        if (!await IsReachableAsync(org.WebsiteUrl, ct))
        {
            _logger.LogInformation("Auto-verify failed for org {OrgId}: website {Url} did not return 2xx.", org.Id, org.WebsiteUrl);
            return false;
        }

        // At least one social media URL must be present and return 2xx
        var socialUrl = ResolveSocialUrl(org);
        if (socialUrl is null)
        {
            _logger.LogInformation("Auto-verify failed for org {OrgId}: no social media details provided.", org.Id);
            return false;
        }

        if (!await IsReachableAsync(socialUrl, ct))
        {
            _logger.LogInformation("Auto-verify failed for org {OrgId}: social URL {Url} did not return 2xx.", org.Id, socialUrl);
            return false;
        }

        return true;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string? ResolveSocialUrl(Organisation org)
    {
        if (!string.IsNullOrWhiteSpace(org.LinkedInUrl))
            return ResolveUrl(org.LinkedInUrl, "https://www.linkedin.com/company/");

        if (!string.IsNullOrWhiteSpace(org.TwitterHandle))
            return $"https://x.com/{StripAt(org.TwitterHandle)}";

        if (!string.IsNullOrWhiteSpace(org.FacebookUrl))
            return ResolveUrl(org.FacebookUrl, "https://www.facebook.com/");

        if (!string.IsNullOrWhiteSpace(org.TikTokHandle))
            return $"https://www.tiktok.com/@{StripAt(org.TikTokHandle)}";

        if (!string.IsNullOrWhiteSpace(org.InstagramHandle))
            return $"https://www.instagram.com/{StripAt(org.InstagramHandle)}/";

        if (!string.IsNullOrWhiteSpace(org.YouTubeUrl))
            return ResolveUrl(org.YouTubeUrl, "https://www.youtube.com/@");

        return null;
    }

    /// <summary>
    /// If the value already looks like an HTTP URL, return it as-is.
    /// Otherwise, treat it as a slug/handle and append to baseUrl.
    /// </summary>
    private static string ResolveUrl(string value, string baseUrl)
    {
        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return value;

        return baseUrl + value.TrimStart('/');
    }

    private static string StripAt(string handle)
        => handle.TrimStart('@');

    private async Task<bool> IsReachableAsync(string url, CancellationToken ct)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            return false;

        try
        {
            using var client = _httpClientFactory.CreateClient("AutoVerifier");

            // Try HEAD first (lighter); fall back to GET if method not allowed
            using var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
            using var headResponse = await client.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead, ct);

            if (headResponse.IsSuccessStatusCode)
                return true;

            if ((int)headResponse.StatusCode == 405)
            {
                using var getResponse = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                return getResponse.IsSuccessStatusCode;
            }

            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            _logger.LogInformation("Auto-verify URL check failed for {Url}: {Message}", url, ex.Message);
            return false;
        }
    }
}
