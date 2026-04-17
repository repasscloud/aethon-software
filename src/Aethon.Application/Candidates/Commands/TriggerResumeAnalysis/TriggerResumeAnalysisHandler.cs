using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Candidates.Commands.TriggerResumeAnalysis;

public sealed class TriggerResumeAnalysisHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TriggerResumeAnalysisHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> HandleAsync(Guid resumeId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure("auth.unauthenticated", "Not authenticated.");

        var resume = await _db.JobSeekerResumes
            .Include(r => r.JobSeekerProfile)
            .FirstOrDefaultAsync(r => r.Id == resumeId, ct);

        if (resume is null || resume.JobSeekerProfile.UserId != _currentUser.UserId)
            return Result.Failure("resumes.not_found", "Resume not found.");

        var existing = await _db.ResumeAnalyses
            .FirstOrDefaultAsync(a => a.JobSeekerResumeId == resumeId, ct);

        var now = _dateTimeProvider.UtcNow;

        if (existing is not null)
        {
            // Reset to pending for re-analysis
            existing.Status = ResumeAnalysisStatus.Pending;
            existing.HeadlineSuggestion = null;
            existing.SummaryExtract = null;
            existing.SkillsJson = null;
            existing.ExperienceLevel = null;
            existing.YearsExperience = null;
            existing.AnalysedUtc = null;
            existing.AnalysisError = null;
            existing.UpdatedByUserId = _currentUser.UserId;
            existing.UpdatedUtc = now;
        }
        else
        {
            _db.ResumeAnalyses.Add(new ResumeAnalysis
            {
                Id = Guid.NewGuid(),
                JobSeekerResumeId = resumeId,
                StoredFileId = resume.StoredFileId,
                Status = ResumeAnalysisStatus.Pending,
                CreatedByUserId = _currentUser.UserId,
                CreatedUtc = now
            });
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
