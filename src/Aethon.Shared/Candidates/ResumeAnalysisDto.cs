using Aethon.Shared.Enums;

namespace Aethon.Shared.Candidates;

public sealed class ResumeAnalysisDto
{
    public Guid ResumeId { get; set; }
    public ResumeAnalysisStatus Status { get; set; }
    public string? HeadlineSuggestion { get; set; }
    public string? SummaryExtract { get; set; }
    public List<string> Skills { get; set; } = [];
    public string? ExperienceLevel { get; set; }
    public int? YearsExperience { get; set; }
    public DateTime? AnalysedUtc { get; set; }
}
