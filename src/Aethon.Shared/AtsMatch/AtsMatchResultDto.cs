namespace Aethon.Shared.AtsMatch;

/// <summary>
/// Returned by GET /api/v1/applications/{id}/match and embedded in application detail DTOs.
/// Visible to both employers and candidates (candidates see score/summary; internal fields omitted).
/// </summary>
public sealed class AtsMatchResultDto
{
    public string Status { get; init; } = string.Empty;      // "Pending", "Processing", "Completed", "Failed"
    public string? Provider { get; init; }                    // "Claude" or "Ollama"
    public int? OverallScore { get; init; }                   // 0–100, null if not yet matched
    public string? Recommendation { get; init; }              // "StrongMatch" etc., null if pending
    public AtsMatchDimensionScoresDto? DimensionScores { get; init; }
    public List<string> Strengths { get; init; } = [];
    public List<string> Gaps { get; init; } = [];
    public string? Summary { get; init; }
    public float? Confidence { get; init; }
    public string? ModelUsed { get; init; }
    public DateTime? ProcessedUtc { get; init; }
}

public sealed class AtsMatchDimensionScoresDto
{
    public int? Skills { get; init; }
    public int? Experience { get; init; }
    public int? Location { get; init; }
    public int? Salary { get; init; }
    public int? Qualifications { get; init; }
    public int? WorkRights { get; init; }
}
