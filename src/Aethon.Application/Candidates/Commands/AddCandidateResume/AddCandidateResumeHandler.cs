using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Candidates;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Candidates.Commands.AddCandidateResume;

public sealed class AddCandidateResumeHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AddCandidateResumeHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<CandidateResumeDto>> HandleAsync(
        AddCandidateResumeCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<CandidateResumeDto>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var currentUserId = _currentUserAccessor.UserId;

        var storedFile = await _dbContext.StoredFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == command.StoredFileId &&
                     x.UploadedByUserId == currentUserId,
                cancellationToken);

        if (storedFile is null)
        {
            return Result<CandidateResumeDto>.Failure(
                "files.not_found",
                "The selected file was not found.");
        }

        var utcNow = _dateTimeProvider.UtcNow;

        var profile = await _dbContext.JobSeekerProfiles
            .Include(x => x.Resumes)
            .FirstOrDefaultAsync(x => x.UserId == currentUserId, cancellationToken);

        if (profile is null)
        {
            profile = new JobSeekerProfile
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                CreatedUtc = utcNow,
                CreatedByUserId = currentUserId
            };

            _dbContext.JobSeekerProfiles.Add(profile);
        }

        var alreadyLinked = profile.Resumes.Any(x => x.StoredFileId == command.StoredFileId && x.IsActive);
        if (alreadyLinked)
        {
            return Result<CandidateResumeDto>.Failure(
                "candidates.resume.already_exists",
                "That file is already linked as an active resume.");
        }

        var shouldBeDefault = command.IsDefault || !profile.Resumes.Any(x => x.IsActive);

        if (shouldBeDefault)
        {
            foreach (var existingResume in profile.Resumes.Where(x => x.IsActive && x.IsDefault))
            {
                existingResume.IsDefault = false;
                existingResume.UpdatedUtc = utcNow;
                existingResume.UpdatedByUserId = currentUserId;
            }
        }

        var resume = new JobSeekerResume
        {
            Id = Guid.NewGuid(),
            JobSeekerProfileId = profile.Id,
            StoredFileId = storedFile.Id,
            Name = command.Name.Trim(),
            Description = Normalize(command.Description),
            IsDefault = shouldBeDefault,
            IsActive = true,
            CreatedUtc = utcNow,
            CreatedByUserId = currentUserId
        };

        _dbContext.JobSeekerResumes.Add(resume);

        profile.LastProfileUpdatedUtc = utcNow;
        profile.UpdatedUtc = utcNow;
        profile.UpdatedByUserId = currentUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<CandidateResumeDto>.Success(new CandidateResumeDto
        {
            Id = resume.Id,
            StoredFileId = storedFile.Id,
            Name = resume.Name,
            Description = resume.Description,
            IsDefault = resume.IsDefault,
            IsActive = resume.IsActive,
            OriginalFileName = storedFile.OriginalFileName,
            ContentType = storedFile.ContentType,
            LengthBytes = storedFile.LengthBytes
        });
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
