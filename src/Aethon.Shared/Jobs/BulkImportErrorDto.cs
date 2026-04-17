namespace Aethon.Shared.Jobs;

public sealed class BulkImportErrorDto
{
    /// <summary>Zero-based position of the record in the submitted batch.</summary>
    public int Index { get; init; }

    /// <summary>ExternalId from the source feed, if it could be read.</summary>
    public string? ExternalId { get; init; }

    /// <summary>SourceSite from the source feed, if it could be read.</summary>
    public string? SourceSite { get; init; }

    /// <summary>Human-readable reason the record was rejected.</summary>
    public string Reason { get; init; } = string.Empty;
}
