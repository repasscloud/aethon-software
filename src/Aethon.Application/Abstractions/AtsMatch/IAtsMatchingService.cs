using Aethon.Shared.Enums;

namespace Aethon.Application.Abstractions.AtsMatch;

public interface IAtsMatchingService
{
    AtsMatchProvider Provider { get; }

    Task<AtsMatchResponse> MatchAsync(string payloadJson, CancellationToken ct = default);
}

public sealed record AtsMatchResponse
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? ModelUsed { get; init; }
    public int OverallScore { get; init; }
    public int? SkillsScore { get; init; }
    public int? ExperienceScore { get; init; }
    public int? LocationScore { get; init; }
    public int? SalaryScore { get; init; }
    public int? QualificationsScore { get; init; }
    public int? WorkRightsScore { get; init; }
    public AtsMatchRecommendation Recommendation { get; init; }
    public List<string> Strengths { get; init; } = [];
    public List<string> Gaps { get; init; } = [];
    public string? Summary { get; init; }
    public float? Confidence { get; init; }
    public string? RawResponseJson { get; init; }
    public int? TokensUsed { get; init; }
}
