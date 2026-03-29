using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Applications.Services;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Applications;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Applications.Queries.GetApplicationFiles;

public sealed class GetApplicationFilesHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly ApplicationAccessService _access;

    public GetApplicationFilesHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        ApplicationAccessService access)
    {
        _db = db;
        _currentUser = currentUser;
        _access = access;
    }

    public async Task<Result<IReadOnlyList<ApplicationFileDto>>> HandleAsync(
        GetApplicationFilesQuery query,
        CancellationToken ct = default)
    {
        var canManage = await _access.CanManageApplicationAsync(
            _currentUser.UserId,
            query.ApplicationId,
            ct);

        var canViewOwn = await _access.CanViewOwnApplicationAsync(
            _currentUser.UserId,
            query.ApplicationId,
            ct);

        if (!canManage && !canViewOwn)
        {
            return Result<IReadOnlyList<ApplicationFileDto>>.Failure(
                "applications.forbidden",
                "Not allowed.");
        }

        var items = await _db.JobApplicationAttachments
            .AsNoTracking()
            .Where(x => x.JobApplicationId == query.ApplicationId)
            .Select(x => new ApplicationFileDto
            {
                Id = x.Id,
                StoredFileId = x.StoredFileId,
                Category = x.Category,
                Notes = x.Notes,
                CreatedUtc = x.CreatedUtc,
                OriginalFileName = x.StoredFile.OriginalFileName,
                ContentType = x.StoredFile.ContentType,
                LengthBytes = x.StoredFile.LengthBytes
            })
            .OrderByDescending(x => x.CreatedUtc)
            .ToListAsync(ct);

        return Result<IReadOnlyList<ApplicationFileDto>>.Success(items);
    }
}
