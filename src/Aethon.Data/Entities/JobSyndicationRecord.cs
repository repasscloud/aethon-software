namespace Aethon.Data.Entities;

public class JobSyndicationRecord : EntityBase
{
    public Guid JobId { get; set; }
    public Job Job { get; set; } = null!;

    /// <summary>e.g. "GoogleIndexing", "Indeed"</summary>
    public string Provider { get; set; } = null!;

    /// <summary>"Success" or "Failed"</summary>
    public string Status { get; set; } = null!;

    /// <summary>External notification ID returned by the provider.</summary>
    public string? ExternalRef { get; set; }

    public DateTime SubmittedUtc { get; set; }
    public DateTime? LastAttemptUtc { get; set; }
    public string? LastErrorMessage { get; set; }
}
