namespace Aethon.Application.Import.Commands.ImportJobs;

public sealed class ImportJobResult
{
    /// <summary>The unique identifier of the newly created (or already-existing) job record.</summary>
    public Guid JobId { get; init; }

    /// <summary>The ExternalReference value used to identify the job in the source system.</summary>
    public string ExternalReference { get; init; } = string.Empty;

    /// <summary>True when the job already existed and was skipped (no duplicate created).</summary>
    public bool WasDuplicate { get; init; }
}
