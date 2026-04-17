using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

/// <summary>
/// Work queue item for AI candidate matching.
/// Created for every job application at submission time.
/// Provider determines which background worker processes it:
///   Ollama  = free tier, processed by OllamaAtsMatchWorker
///   Claude  = paid tier (HasAiCandidateMatching = true), processed by AtsMatchClaudeWorker
/// </summary>
public sealed class AtsMatchQueueItem : EntityBase
{
    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public Guid JobId { get; set; }
    public Guid CandidateUserId { get; set; }

    /// <summary>Which LLM provider will process this item.</summary>
    public AtsMatchProvider Provider { get; set; }

    /// <summary>Higher priority items are processed first within the same provider bucket.</summary>
    public int Priority { get; set; }

    public AtsMatchStatus Status { get; set; } = AtsMatchStatus.Pending;

    /// <summary>Number of processing attempts made. Max 3 before marking Failed.</summary>
    public int Attempts { get; set; }

    public DateTime? LastAttemptUtc { get; set; }
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// JSON snapshot of the AtsMatchPayload (job + candidate data) captured at enqueue time.
    /// Workers use this directly — no re-querying needed.
    /// </summary>
    public string PayloadJson { get; set; } = string.Empty;

    public DateTime? ProcessedUtc { get; set; }

    public AtsMatchResult? Result { get; set; }
}
