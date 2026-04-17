namespace Aethon.Application.Import.Commands.ImportJobs;

/// <summary>
/// Returned by <see cref="ImportJobsHandler.HandleBulkAsync"/> so the endpoint
/// can separately report per-record successes and per-record handler failures.
/// </summary>
public sealed class BulkHandlerResult
{
    public List<ImportJobResult> Succeeded { get; init; } = [];
    public List<BulkHandlerFailure> Failed  { get; init; } = [];
}

public sealed class BulkHandlerFailure
{
    public string? SourceSite  { get; init; }
    public string? ExternalId  { get; init; }
    public string  Reason      { get; init; } = string.Empty;
}
