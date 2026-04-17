namespace Aethon.Shared.Enums;

/// <summary>
/// Human-readable recommendation derived from the overall ATS match score.
/// StrongMatch = 80–100, GoodMatch = 60–79, PartialMatch = 40–59, PoorMatch = 20–39, NotSuitable = 0–19.
/// </summary>
public enum AtsMatchRecommendation
{
    StrongMatch  = 1,
    GoodMatch    = 2,
    PartialMatch = 3,
    PoorMatch    = 4,
    NotSuitable  = 5
}
