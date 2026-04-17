namespace Aethon.Shared.Jobs;

public sealed class BulkImportResponseDto
{
    /// <summary>Total records received in the batch.</summary>
    public int Received { get; init; }

    /// <summary>Records that were newly created.</summary>
    public int Imported { get; init; }

    /// <summary>Records that updated an existing job.</summary>
    public int Updated { get; init; }

    /// <summary>Records that matched an existing job with no changes needed.</summary>
    public int Skipped { get; init; }

    /// <summary>Records that could not be processed (parse error or validation failure).</summary>
    public int Failed { get; init; }

    /// <summary>Per-record error detail for every failed record.</summary>
    public List<BulkImportErrorDto> Errors { get; init; } = [];
}
