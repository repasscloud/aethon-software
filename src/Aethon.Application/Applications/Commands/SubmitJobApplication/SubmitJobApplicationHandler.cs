using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Caching;
using Aethon.Application.Abstractions.Email;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Caching;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Applications.Commands.SubmitJobApplication;

public sealed class SubmitJobApplicationHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAppCache _cache;
    private readonly IEmailService _emailService;

    public SubmitJobApplicationHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IDateTimeProvider dateTimeProvider,
        IAppCache cache,
        IEmailService emailService)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _dateTimeProvider = dateTimeProvider;
        _cache = cache;
        _emailService = emailService;
    }

    public async Task<Result<SubmitJobApplicationResult>> HandleAsync(
        SubmitJobApplicationCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<SubmitJobApplicationResult>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var currentUserId = _currentUserAccessor.UserId;

        var job = await _dbContext.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == command.JobId &&
                     x.Status == JobStatus.Published,
                cancellationToken);

        if (job is null)
        {
            return Result<SubmitJobApplicationResult>.Failure(
                "jobs.not_found",
                "The requested job was not found or is not open for applications.");
        }

        if (job.ApplyByUtc.HasValue && job.ApplyByUtc.Value < _dateTimeProvider.UtcNow)
        {
            return Result<SubmitJobApplicationResult>.Failure(
                "applications.job_closed",
                "Applications are closed for this job.");
        }

        var profileExists = await _dbContext.JobSeekerProfiles
            .AsNoTracking()
            .AnyAsync(x => x.UserId == currentUserId, cancellationToken);

        if (!profileExists)
        {
            return Result<SubmitJobApplicationResult>.Failure(
                "applications.profile_required",
                "A job seeker profile is required before applying.");
        }

        var existingApplicationExists = await _dbContext.JobApplications
            .AsNoTracking()
            .AnyAsync(x => x.JobId == command.JobId && x.UserId == currentUserId, cancellationToken);

        if (existingApplicationExists)
        {
            return Result<SubmitJobApplicationResult>.Failure(
                "applications.duplicate",
                "The current user has already applied for this job.");
        }

        if (command.ResumeFileId.HasValue)
        {
            var resumeExists = await _dbContext.StoredFiles
                .AsNoTracking()
                .AnyAsync(x => x.Id == command.ResumeFileId.Value, cancellationToken);

            if (!resumeExists)
            {
                return Result<SubmitJobApplicationResult>.Failure(
                    "applications.resume_not_found",
                    "The selected resume file was not found.");
            }
        }

        var utcNow = _dateTimeProvider.UtcNow;
        var applicationId = Guid.NewGuid();

        var application = new JobApplication
        {
            Id = applicationId,
            JobId = command.JobId,
            UserId = currentUserId,
            Status = ApplicationStatus.Submitted,
            ResumeFileId = command.ResumeFileId,
            CoverLetter = Normalize(command.CoverLetter),
            Source = Normalize(command.Source) ?? "AethonJobBoard",
            SubmittedUtc = utcNow,
            LastStatusChangedUtc = utcNow,
            LastActivityUtc = utcNow,
            CreatedUtc = utcNow,
            CreatedByUserId = currentUserId
        };

        var historyEntry = new JobApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            JobApplicationId = applicationId,
            FromStatus = null,
            ToStatus = ApplicationStatus.Submitted,
            ChangedByUserId = currentUserId,
            ChangedUtc = utcNow,
            CreatedUtc = utcNow,
            CreatedByUserId = currentUserId,
            Notes = "Application submitted."
        };

        _dbContext.JobApplications.Add(application);
        _dbContext.JobApplicationStatusHistoryEntries.Add(historyEntry);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByPrefixAsync(CacheKeys.MyApplicationsPrefix(currentUserId), cancellationToken);
        await _cache.RemoveByPrefixAsync(CacheKeys.JobApplicationsPrefix(command.JobId), cancellationToken);

        var applicantEmail = await _dbContext.Users
            .Where(u => u.Id == currentUserId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(applicantEmail))
        {
            await _emailService.SendAsync(new EmailMessage
            {
                ToEmail = applicantEmail,
                Subject = $"Application submitted — {job.Title}",
                TextBody = $"Your application for \"{job.Title}\" has been submitted successfully.\n\nYou can track the status of your application in My applications on Aethon.",
                HtmlBody = $"""
                    <!DOCTYPE html><html><body style="font-family:Arial,sans-serif;line-height:1.5;">
                    <h2>Application submitted</h2>
                    <p>Your application for <strong>{job.Title}</strong> has been submitted successfully.</p>
                    <p>You can track the status of your application in <em>My applications</em> on Aethon.</p>
                    </body></html>
                    """
            }, cancellationToken);
        }

        return Result<SubmitJobApplicationResult>.Success(new SubmitJobApplicationResult
        {
            Id = application.Id,
            JobId = application.JobId,
            Status = application.Status,
            SubmittedUtc = application.SubmittedUtc
        });
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
