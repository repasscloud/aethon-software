namespace Aethon.Shared.Utilities;

public static class JobUrlHelper
{
    /// <summary>
    /// Builds the canonical public URL for a job detail page.
    /// Used by both the API (Google Indexing API payload) and the Web (JSON-LD url field)
    /// to guarantee identical URLs. Divergent URLs break Google indexing.
    /// </summary>
    public static string BuildPublicUrl(string webBaseUrl, Guid jobId)
        => $"{webBaseUrl.TrimEnd('/')}/jobs/{jobId}";
}
