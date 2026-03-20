using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Candidates.Commands.SetDefaultCandidateResume;

public sealed class SetDefaultCandidateResumeHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SetDefaultCandidateResumeHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> HandleAsync(
        SetDefaultCandidateResumeCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var currentUserId = _currentUserAccessor.UserId;

        var profile = await _dbContext.JobSeekerProfiles
            .Include(x => x.Resumes)
            .FirstOrDefaultAsync(x => x.UserId == currentUserId, cancellationToken);

        if (profile is null)
        {
            return Result.Failure(
                "candidates.profile.not_found",
                "Candidate profile was not found.");
        }

        var targetResume = profile.Resumes.FirstOrDefault(x => x.Id == command.ResumeId && x.IsActive);
        if (targetResume is null)
        {
            return Result.Failure(
                "candidates.resume.not_found",
                "Resume was not found.");
        }

        var utcNow = _dateTimeProvider.UtcNow;

        foreach (var resume in profile.Resumes.Where(x => x.IsActive))
        {
            resume.IsDefault = resume.Id == command.ResumeId;
            resume.UpdatedUtc = utcNow;
            resume.UpdatedByUserId = currentUserId;
        }

        profile.LastProfileUpdatedUtc = utcNow;
        profile.UpdatedUtc = utcNow;
        profile.UpdatedByUserId = currentUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
