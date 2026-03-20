namespace Aethon.Application.Abstractions.ResumeAnalysis;

public interface IResumeAnalysisService
{
    Task<ResumeAnalysisResult> AnalyseAsync(
        string fileName,
        string contentType,
        byte[] fileBytes,
        CancellationToken ct = default);
}

public sealed class ResumeAnalysisResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? HeadlineSuggestion { get; init; }
    public string? SummaryExtract { get; init; }
    public List<string> Skills { get; init; } = [];
    public string? ExperienceLevel { get; init; }
    public int? YearsExperience { get; init; }
}
