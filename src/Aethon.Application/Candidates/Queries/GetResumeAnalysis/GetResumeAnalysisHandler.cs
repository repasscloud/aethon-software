using System.Text.Json;
using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Candidates;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Candidates.Queries.GetResumeAnalysis;

public sealed class GetResumeAnalysisHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public GetResumeAnalysisHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<ResumeAnalysisDto>> HandleAsync(Guid resumeId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<ResumeAnalysisDto>.Failure("auth.unauthenticated", "Not authenticated.");

        var resume = await _db.JobSeekerResumes
            .AsNoTracking()
            .Include(r => r.JobSeekerProfile)
            .FirstOrDefaultAsync(r => r.Id == resumeId, ct);

        if (resume is null || resume.JobSeekerProfile.UserId != _currentUser.UserId)
            return Result<ResumeAnalysisDto>.Failure("resumes.not_found", "Resume not found.");

        var analysis = await _db.ResumeAnalyses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.JobSeekerResumeId == resumeId, ct);

        if (analysis is null)
            return Result<ResumeAnalysisDto>.Failure("resumes.analysis_not_found", "No analysis found for this resume.");

        var skills = new List<string>();
        if (!string.IsNullOrWhiteSpace(analysis.SkillsJson))
        {
            try { skills = JsonSerializer.Deserialize<List<string>>(analysis.SkillsJson) ?? []; }
            catch { /* ignore malformed JSON */ }
        }

        return Result<ResumeAnalysisDto>.Success(new ResumeAnalysisDto
        {
            ResumeId = resumeId,
            Status = analysis.Status,
            HeadlineSuggestion = analysis.HeadlineSuggestion,
            SummaryExtract = analysis.SummaryExtract,
            Skills = skills,
            ExperienceLevel = analysis.ExperienceLevel,
            YearsExperience = analysis.YearsExperience,
            AnalysedUtc = analysis.AnalysedUtc
        });
    }
}
