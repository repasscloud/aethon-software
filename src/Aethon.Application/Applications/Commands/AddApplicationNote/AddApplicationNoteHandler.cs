using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Activity.Services;
using Aethon.Application.Applications.Services;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Applications.Commands.AddApplicationNote;

public sealed class AddApplicationNoteHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ApplicationAccessService _applicationAccessService;
    private readonly ActivityLogWriter _activityLogWriter;

    public AddApplicationNoteHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IDateTimeProvider dateTimeProvider,
        ApplicationAccessService applicationAccessService,
        ActivityLogWriter activityLogWriter)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _dateTimeProvider = dateTimeProvider;
        _applicationAccessService = applicationAccessService;
        _activityLogWriter = activityLogWriter;
    }

    public async Task<Result<AddApplicationNoteResult>> HandleAsync(
        AddApplicationNoteCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<AddApplicationNoteResult>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var content = Normalize(command.Content);

        if (content is null)
        {
            return Result<AddApplicationNoteResult>.Failure(
                "applications.note.content_required",
                "Note content is required.");
        }

        var currentUserId = _currentUserAccessor.UserId;

        var application = await _dbContext.JobApplications
            .Include(x => x.Job)
            .SingleOrDefaultAsync(x => x.Id == command.ApplicationId, cancellationToken);

        if (application is null)
        {
            return Result<AddApplicationNoteResult>.Failure(
                "applications.not_found",
                "The requested application was not found.");
        }

        var canManage = await _applicationAccessService.CanManageApplicationAsync(
            currentUserId,
            command.ApplicationId,
            cancellationToken);

        if (!canManage)
        {
            return Result<AddApplicationNoteResult>.Failure(
                "applications.forbidden",
                "The current user cannot add notes to this application.");
        }

        var utcNow = _dateTimeProvider.UtcNow;

        var note = new JobApplicationNote
        {
            Id = Guid.NewGuid(),
            JobApplicationId = application.Id,
            Content = content,
            CreatedUtc = utcNow,
            CreatedByUserId = currentUserId
        };

        _dbContext.JobApplicationNotes.Add(note);

        application.LastActivityUtc = utcNow;
        application.UpdatedUtc = utcNow;
        application.UpdatedByUserId = currentUserId;

        await _activityLogWriter.WriteAsync(
            entityType: nameof(JobApplication),
            entityId: application.Id,
            action: "NoteAdded",
            performedUtc: utcNow,
            performedByUserId: currentUserId,
            organisationId: application.Job.OwnedByOrganisationId,
            summary: "Internal note added to application.",
            details: Truncate(content, 500),
            cancellationToken: cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<AddApplicationNoteResult>.Success(new AddApplicationNoteResult
        {
            Id = note.Id,
            ApplicationId = note.JobApplicationId,
            Content = note.Content,
            CreatedUtc = note.CreatedUtc
        });
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
