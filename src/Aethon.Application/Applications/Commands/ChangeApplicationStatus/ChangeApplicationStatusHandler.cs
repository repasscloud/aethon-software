using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Caching;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Activity.Services;
using Aethon.Application.Applications.Services;
using Aethon.Application.Common.Caching;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Applications.Commands.ChangeApplicationStatus;

public sealed class ChangeApplicationStatusHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ApplicationAccessService _applicationAccessService;
    private readonly ApplicationWorkflowService _applicationWorkflowService;
    private readonly ActivityLogWriter _activityLogWriter;
    private readonly IAppCache _cache;

    public ChangeApplicationStatusHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IDateTimeProvider dateTimeProvider,
        ApplicationAccessService applicationAccessService,
        ApplicationWorkflowService applicationWorkflowService,
        ActivityLogWriter activityLogWriter,
        IAppCache cache)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _dateTimeProvider = dateTimeProvider;
        _applicationAccessService = applicationAccessService;
        _applicationWorkflowService = applicationWorkflowService;
        _activityLogWriter = activityLogWriter;
        _cache = cache;
    }

    public async Task<Result> HandleAsync(
        ChangeApplicationStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var currentUserId = _currentUserAccessor.UserId;
        var normalizedReason = Normalize(command.Reason);
        var normalizedNotes = Normalize(command.Notes);

        var canManage = await _applicationAccessService.CanManageApplicationAsync(
            currentUserId,
            command.ApplicationId,
            cancellationToken);

        if (!canManage)
        {
            return Result.Failure(
                "applications.forbidden",
                "The current user cannot manage this application.");
        }

        var application = await _dbContext.JobApplications
            .Include(x => x.Job)
            .SingleOrDefaultAsync(x => x.Id == command.ApplicationId, cancellationToken);

        if (application is null)
        {
            return Result.Failure(
                "applications.not_found",
                "The requested application was not found.");
        }

        var transitionValidation = _applicationWorkflowService.ValidateStatusTransition(
            application.Status,
            command.Status,
            normalizedReason);

        if (!transitionValidation.Succeeded)
        {
            return transitionValidation;
        }

        var utcNow = _dateTimeProvider.UtcNow;
        var previousStatus = application.Status;

        _applicationWorkflowService.ApplyStatusSideEffects(
            application,
            command.Status,
            normalizedReason,
            utcNow,
            currentUserId);

        application.UpdatedByUserId = currentUserId;

        var historyEntry = new JobApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            JobApplicationId = application.Id,
            FromStatus = previousStatus,
            ToStatus = command.Status,
            Reason = normalizedReason,
            Notes = normalizedNotes,
            ChangedByUserId = currentUserId,
            ChangedUtc = utcNow,
            CreatedUtc = utcNow,
            CreatedByUserId = currentUserId
        };

        _dbContext.JobApplicationStatusHistoryEntries.Add(historyEntry);

        await _activityLogWriter.WriteAsync(
            entityType: nameof(JobApplication),
            entityId: application.Id,
            action: "StatusChanged",
            performedUtc: utcNow,
            performedByUserId: currentUserId,
            organisationId: application.Job.OwnedByOrganisationId,
            summary: $"Application status changed from {previousStatus} to {command.Status}.",
            details: BuildDetails(previousStatus.ToString(), command.Status.ToString(), normalizedReason, normalizedNotes),
            cancellationToken: cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await InvalidateAsync(application, cancellationToken);

        return Result.Success();
    }

    private async Task InvalidateAsync(
        JobApplication application,
        CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync(CacheKeys.ApplicationDetail(application.Id), cancellationToken);
        await _cache.RemoveAsync(CacheKeys.ApplicationTimeline(application.Id), cancellationToken);
        await _cache.RemoveByPrefixAsync(CacheKeys.MyApplicationsPrefix(application.UserId), cancellationToken);
        await _cache.RemoveByPrefixAsync(CacheKeys.JobApplicationsPrefix(application.JobId), cancellationToken);
    }

    private static string BuildDetails(
        string fromStatus,
        string toStatus,
        string? reason,
        string? notes)
    {
        var parts = new List<string>
        {
            $"from={fromStatus}",
            $"to={toStatus}"
        };

        if (!string.IsNullOrWhiteSpace(reason))
        {
            parts.Add($"reason={reason}");
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            parts.Add($"notes={notes}");
        }

        return string.Join("; ", parts);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
