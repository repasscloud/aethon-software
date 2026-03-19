using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Activity.Services;
using Aethon.Application.Applications.Services;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Applications.Commands.ScheduleInterview;

public sealed class ScheduleInterviewHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ApplicationAccessService _applicationAccessService;
    private readonly ApplicationWorkflowService _applicationWorkflowService;
    private readonly ActivityLogWriter _activityLogWriter;

    public ScheduleInterviewHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IDateTimeProvider dateTimeProvider,
        ApplicationAccessService applicationAccessService,
        ApplicationWorkflowService applicationWorkflowService,
        ActivityLogWriter activityLogWriter)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _dateTimeProvider = dateTimeProvider;
        _applicationAccessService = applicationAccessService;
        _applicationWorkflowService = applicationWorkflowService;
        _activityLogWriter = activityLogWriter;
    }

    public async Task<Result<ScheduleInterviewResult>> HandleAsync(
        ScheduleInterviewCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<ScheduleInterviewResult>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        if (command.ScheduledEndUtc <= command.ScheduledStartUtc)
        {
            return Result<ScheduleInterviewResult>.Failure(
                "applications.interview.invalid_range",
                "Interview end time must be after the start time.");
        }

        var currentUserId = _currentUserAccessor.UserId;

        var application = await _dbContext.JobApplications
            .Include(x => x.Job)
            .SingleOrDefaultAsync(x => x.Id == command.ApplicationId, cancellationToken);

        if (application is null)
        {
            return Result<ScheduleInterviewResult>.Failure(
                "applications.not_found",
                "The requested application was not found.");
        }

        var canManage = await _applicationAccessService.CanManageApplicationAsync(
            currentUserId,
            command.ApplicationId,
            cancellationToken);

        if (!canManage)
        {
            return Result<ScheduleInterviewResult>.Failure(
                "applications.forbidden",
                "The current user cannot schedule interviews for this application.");
        }

        var utcNow = _dateTimeProvider.UtcNow;
        var normalizedTitle = Normalize(command.Title);
        var normalizedLocation = Normalize(command.Location);
        var normalizedMeetingUrl = Normalize(command.MeetingUrl);
        var normalizedNotes = Normalize(command.Notes);

        var interview = new JobApplicationInterview
        {
            Id = Guid.NewGuid(),
            JobApplicationId = application.Id,
            Type = command.Type,
            Status = InterviewStatus.Scheduled,
            Title = normalizedTitle,
            Location = normalizedLocation,
            MeetingUrl = normalizedMeetingUrl,
            Notes = normalizedNotes,
            ScheduledStartUtc = command.ScheduledStartUtc,
            ScheduledEndUtc = command.ScheduledEndUtc,
            CreatedUtc = utcNow,
            CreatedByUserId = currentUserId
        };

        _dbContext.JobApplicationInterviews.Add(interview);

        var previousStatus = application.Status;
        var movedToInterview = false;

        if (application.Status != ApplicationStatus.Interview)
        {
            var transitionValidation = _applicationWorkflowService.ValidateStatusTransition(
                application.Status,
                ApplicationStatus.Interview);

            if (!transitionValidation.Succeeded)
            {
                return Result<ScheduleInterviewResult>.Failure(
                    transitionValidation.ErrorCode ?? "applications.interview.invalid_status",
                    transitionValidation.ErrorMessage ?? "Interview cannot be scheduled from the current application status.");
            }

            _applicationWorkflowService.ApplyStatusSideEffects(
                application,
                ApplicationStatus.Interview,
                null,
                utcNow,
                currentUserId);

            application.UpdatedByUserId = currentUserId;

            var historyEntry = new JobApplicationStatusHistory
            {
                Id = Guid.NewGuid(),
                JobApplicationId = application.Id,
                FromStatus = previousStatus,
                ToStatus = ApplicationStatus.Interview,
                Reason = null,
                Notes = "Application moved to Interview due to scheduled interview.",
                ChangedByUserId = currentUserId,
                ChangedUtc = utcNow,
                CreatedUtc = utcNow,
                CreatedByUserId = currentUserId
            };

            _dbContext.JobApplicationStatusHistoryEntries.Add(historyEntry);
            movedToInterview = true;
        }
        else
        {
            application.LastActivityUtc = utcNow;
            application.UpdatedUtc = utcNow;
            application.UpdatedByUserId = currentUserId;
        }

        await _activityLogWriter.WriteAsync(
            entityType: nameof(JobApplication),
            entityId: application.Id,
            action: "InterviewScheduled",
            performedUtc: utcNow,
            performedByUserId: currentUserId,
            organisationId: application.Job.OwnedByOrganisationId,
            summary: $"Interview scheduled for {command.ScheduledStartUtc:u}.",
            details: BuildInterviewDetails(command.Type, normalizedTitle, normalizedLocation, normalizedMeetingUrl, normalizedNotes),
            cancellationToken: cancellationToken);

        if (movedToInterview)
        {
            await _activityLogWriter.WriteAsync(
                entityType: nameof(JobApplication),
                entityId: application.Id,
                action: "StatusChanged",
                performedUtc: utcNow,
                performedByUserId: currentUserId,
                organisationId: application.Job.OwnedByOrganisationId,
                summary: $"Application status changed from {previousStatus} to {ApplicationStatus.Interview}.",
                details: "from=" + previousStatus + "; to=" + ApplicationStatus.Interview + "; notes=Application moved to Interview due to scheduled interview.",
                cancellationToken: cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<ScheduleInterviewResult>.Success(new ScheduleInterviewResult
        {
            Id = interview.Id,
            ApplicationId = interview.JobApplicationId,
            Type = interview.Type,
            Status = interview.Status,
            Title = interview.Title,
            ScheduledStartUtc = interview.ScheduledStartUtc,
            ScheduledEndUtc = interview.ScheduledEndUtc
        });
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string BuildInterviewDetails(
        InterviewType type,
        string? title,
        string? location,
        string? meetingUrl,
        string? notes)
    {
        var parts = new List<string>
        {
            $"type={type}"
        };

        if (!string.IsNullOrWhiteSpace(title))
        {
            parts.Add($"title={title}");
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            parts.Add($"location={location}");
        }

        if (!string.IsNullOrWhiteSpace(meetingUrl))
        {
            parts.Add($"meetingUrl={meetingUrl}");
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            parts.Add($"notes={notes}");
        }

        return string.Join("; ", parts);
    }
}
