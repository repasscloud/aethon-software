using Microsoft.AspNetCore.WebUtilities;

namespace Aethon.Api.Infrastructure;

public static class AuthRedirectHelper
{
    public static string NormaliseReturnPath(string? value, string fallback = "/")
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        if (!value.StartsWith('/'))
        {
            value = "/" + value;
        }

        if (value.StartsWith("//"))
        {
            return fallback;
        }

        if (Uri.TryCreate(value, UriKind.Absolute, out _))
        {
            return fallback;
        }

        return value;
    }

    public static string BuildLoginRedirect(string webBaseUrl, string returnPath, string error)
    {
        var loginUrl = $"{webBaseUrl}/login";

        var query = new Dictionary<string, string?>
        {
            ["ReturnUrl"] = returnPath,
            ["error"] = error
        };

        return QueryHelpers.AddQueryString(loginUrl, query);
    }
}