using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Applications.Services;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Applications.Commands.AttachApplicationFile;

public sealed class AttachApplicationFileHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IDateTimeProvider _time;
    private readonly ApplicationAccessService _access;

    public AttachApplicationFileHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        IDateTimeProvider time,
        ApplicationAccessService access)
    {
        _db = db;
        _currentUser = currentUser;
        _time = time;
        _access = access;
    }

    public async Task<Result> HandleAsync(
        AttachApplicationFileCommand command,
        CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
        {
            return Result.Failure("auth.unauthenticated", "User not authenticated.");
        }

        var canManage = await _access.CanManageApplicationAsync(
            _currentUser.UserId,
            command.ApplicationId,
            ct);

        if (!canManage)
        {
            return Result.Failure("applications.forbidden", "Not allowed.");
        }

        var fileExists = await _db.StoredFiles
            .AnyAsync(x => x.Id == command.StoredFileId, ct);

        if (!fileExists)
        {
            return Result.Failure("files.not_found", "File not found.");
        }

        var utcNow = _time.UtcNow;

        var entity = new JobApplicationAttachment
        {
            Id = Guid.NewGuid(),
            JobApplicationId = command.ApplicationId,
            StoredFileId = command.StoredFileId,
            Category = command.Category,
            Notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim(),
            CreatedUtc = utcNow,
            CreatedByUserId = _currentUser.UserId
        };

        _db.JobApplicationAttachments.Add(entity);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
