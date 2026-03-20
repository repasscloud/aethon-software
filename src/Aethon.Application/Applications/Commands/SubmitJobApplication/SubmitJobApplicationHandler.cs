using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Integrations;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Application.Integrations.Events;
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
    private readonly IWebhookEventDispatcher _webhookEventDispatcher;

    public SubmitJobApplicationHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IDateTimeProvider dateTimeProvider,
        IWebhookEventDispatcher webhookEventDispatcher)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _dateTimeProvider = dateTimeProvider;
        _webhookEventDispatcher = webhookEventDispatcher;
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

        var profile = await _dbContext.JobSeekerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == currentUserId, cancellationToken);

        if (profile is null)
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

        Guid? resumeFileId = command.ResumeFileId;

        if (!resumeFileId.HasValue)
        {
            resumeFileId = await _dbContext.JobSeekerResumes
                .AsNoTracking()
                .Where(x =>
                    x.JobSeekerProfileId == profile.Id &&
                    x.IsActive &&
                    x.IsDefault)
                .Select(x => (Guid?)x.StoredFileId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!resumeFileId.HasValue)
            {
                resumeFileId = await _dbContext.JobSeekerResumes
                    .AsNoTracking()
                    .Where(x =>
                        x.JobSeekerProfileId == profile.Id &&
                        x.IsActive)
                    .OrderByDescending(x => x.IsDefault)
                    .ThenBy(x => x.CreatedUtc)
                    .Select(x => (Guid?)x.StoredFileId)
                    .FirstOrDefaultAsync(cancellationToken);
            }
        }

        if (resumeFileId.HasValue)
        {
            var resumeExists = await _dbContext.JobSeekerResumes
                .AsNoTracking()
                .AnyAsync(
                    x => x.JobSeekerProfileId == profile.Id &&
                         x.StoredFileId == resumeFileId.Value &&
                         x.IsActive,
                    cancellationToken);

            if (!resumeExists)
            {
                return Result<SubmitJobApplicationResult>.Failure(
                    "applications.resume_not_found",
                    "The selected resume was not found for the current candidate.");
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
            ResumeFileId = resumeFileId,
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

        await _webhookEventDispatcher.QueueAsync(
            job.OwnedByOrganisationId,
            IntegrationEventTypes.ApplicationSubmitted,
            new
            {
                applicationId,
                jobId = command.JobId,
                applicantUserId = currentUserId,
                submittedUtc = utcNow
            },
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

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