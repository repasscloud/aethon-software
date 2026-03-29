using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Applications.Services;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Applications;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Applications.Queries.GetApplicationTimeline;

public sealed class GetApplicationTimelineHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ApplicationAccessService _applicationAccessService;

    public GetApplicationTimelineHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        ApplicationAccessService applicationAccessService)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _applicationAccessService = applicationAccessService;
    }

    public async Task<Result<IReadOnlyList<ApplicationTimelineItemDto>>> HandleAsync(
        GetApplicationTimelineQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<IReadOnlyList<ApplicationTimelineItemDto>>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var currentUserId = _currentUserAccessor.UserId;

        var application = await _dbContext.JobApplications
            .AsNoTracking()
            .Where(x => x.Id == query.ApplicationId)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.SubmittedUtc,
                x.CreatedByUserId,
                ApplicantDisplayName = x.User.DisplayName
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (application is null)
        {
            return Result<IReadOnlyList<ApplicationTimelineItemDto>>.Failure(
                "applications.not_found",
                "The requested application was not found.");
        }

        var canManage = await _applicationAccessService.CanManageApplicationAsync(
            currentUserId,
            query.ApplicationId,
            cancellationToken);

        var canViewOwn = !canManage && application.UserId == currentUserId;

        if (!canManage && !canViewOwn)
        {
            return Result<IReadOnlyList<ApplicationTimelineItemDto>>.Failure(
                "applications.forbidden",
                "The current user cannot view this application timeline.");
        }

        var timelineItems = new List<ApplicationTimelineItemDto>();

        timelineItems.Add(new ApplicationTimelineItemDto
        {
            EventType = "Submitted",
            OccurredUtc = application.SubmittedUtc,
            PerformedByUserId = application.CreatedByUserId,
            PerformedByDisplayName = application.ApplicantDisplayName,
            Title = "Application submitted"
        });

        var statusItems = await _dbContext.JobApplicationStatusHistoryEntries
            .AsNoTracking()
            .Where(x => x.JobApplicationId == query.ApplicationId)
            .Select(x => new ApplicationTimelineItemDto
            {
                EventType = "StatusChanged",
                OccurredUtc = x.ChangedUtc,
                PerformedByUserId = x.ChangedByUserId,
                PerformedByDisplayName = x.ChangedByUser.DisplayName,
                Title = $"Status changed to {x.ToStatus}",
                Description = BuildDescription(x.Reason, x.Notes),
                FromStatus = x.FromStatus.HasValue ? x.FromStatus.Value.ToString() : null,
                ToStatus = x.ToStatus.ToString()
            })
            .ToListAsync(cancellationToken);

        timelineItems.AddRange(statusItems);

        if (canManage)
        {
            var noteItems = await _dbContext.JobApplicationNotes
                .AsNoTracking()
                .Where(x => x.JobApplicationId == query.ApplicationId && !x.IsDeleted)
                .Select(x => new ApplicationTimelineItemDto
                {
                    EventType = "Note",
                    OccurredUtc = x.CreatedUtc,
                    PerformedByUserId = x.CreatedByUserId,
                    PerformedByDisplayName = x.CreatedByUser.DisplayName,
                    Title = "Internal note added",
                    Description = x.Content
                })
                .ToListAsync(cancellationToken);

            timelineItems.AddRange(noteItems);

            var commentItems = await _dbContext.JobApplicationComments
                .AsNoTracking()
                .Where(x => x.JobApplicationId == query.ApplicationId && !x.IsDeleted)
                .Select(x => new ApplicationTimelineItemDto
                {
                    EventType = "Comment",
                    OccurredUtc = x.CreatedUtc,
                    PerformedByUserId = x.CreatedByUserId,
                    PerformedByDisplayName = x.CreatedByUser.DisplayName,
                    Title = x.ParentCommentId.HasValue ? "Comment reply added" : "Comment added",
                    Description = x.Content
                })
                .ToListAsync(cancellationToken);

            timelineItems.AddRange(commentItems);

            var interviewItems = await _dbContext.JobApplicationInterviews
                .AsNoTracking()
                .Where(x => x.JobApplicationId == query.ApplicationId)
                .Select(x => new ApplicationTimelineItemDto
                {
                    EventType = "Interview",
                    OccurredUtc = x.ScheduledStartUtc,
                    PerformedByUserId = x.CreatedByUserId,
                    PerformedByDisplayName = null,
                    Title = x.Title ?? $"{x.Type} interview scheduled",
                    Description = BuildInterviewDescription(x.Location, x.MeetingUrl, x.Notes)
                })
                .ToListAsync(cancellationToken);

            timelineItems.AddRange(interviewItems);
        }

        var orderedItems = timelineItems
            .OrderBy(x => x.OccurredUtc)
            .ToList();

        return Result<IReadOnlyList<ApplicationTimelineItemDto>>.Success(orderedItems);
    }

    private static string? BuildDescription(string? reason, string? notes)
    {
        if (string.IsNullOrWhiteSpace(reason) && string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(reason) && !string.IsNullOrWhiteSpace(notes))
        {
            return $"Reason: {reason}\nNotes: {notes}";
        }

        if (!string.IsNullOrWhiteSpace(reason))
        {
            return $"Reason: {reason}";
        }

        return $"Notes: {notes}";
    }

    private static string? BuildInterviewDescription(
        string? location,
        string? meetingUrl,
        string? notes)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(location))
        {
            parts.Add($"Location: {location.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(meetingUrl))
        {
            parts.Add($"Meeting URL: {meetingUrl.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            parts.Add($"Notes: {notes.Trim()}");
        }

        return parts.Count == 0 ? null : string.Join(" | ", parts);
    }
}
