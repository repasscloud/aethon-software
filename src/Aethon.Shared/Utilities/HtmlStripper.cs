using System.Net;
using System.Text.RegularExpressions;

namespace Aethon.Shared.Utilities;

public static partial class HtmlStripper
{
    [GeneratedRegex("<[^>]*>", RegexOptions.Compiled)]
    private static partial Regex TagPattern();

    [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
    private static partial Regex WhitespacePattern();

    /// <summary>
    /// Strips all HTML tags, decodes HTML entities, and collapses whitespace.
    /// Used for JSON-LD descriptions — the displayed description still uses HTML.
    /// </summary>
    public static string Strip(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var noTags = TagPattern().Replace(html, " ");
        var decoded = WebUtility.HtmlDecode(noTags);
        return WhitespacePattern().Replace(decoded, " ").Trim();
    }
}
