using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Applications;
using Aethon.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Applications.Queries.GetMyApplications;

public sealed class GetMyApplicationsHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public GetMyApplicationsHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<PagedResult<ApplicationSummaryDto>>> HandleAsync(
        GetMyApplicationsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<PagedResult<ApplicationSummaryDto>>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var currentUserId = _currentUserAccessor.UserId;

        var baseQuery = _dbContext.JobApplications
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(x => x.SubmittedUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ApplicationSummaryDto
            {
                Id = x.Id,
                JobId = x.JobId,
                JobTitle = x.Job.Title,
                Status = x.Status,
                StatusReason = x.StatusReason,
                Source = x.Source ?? string.Empty,
                ResumeFileId = x.ResumeFileId,
                Resume = x.ResumeFileId.HasValue
                    ? x.User.JobSeekerProfile != null
                        ? x.User.JobSeekerProfile.Resumes
                            .Where(r => r.StoredFileId == x.ResumeFileId.Value && r.IsActive)
                            .Select(r => new ApplicationResumeDto
                            {
                                Id = r.Id,
                                StoredFileId = r.StoredFileId,
                                Name = r.Name,
                                Description = r.Description,
                                IsDefault = r.IsDefault,
                                OriginalFileName = r.StoredFile.OriginalFileName,
                                ContentType = r.StoredFile.ContentType,
                                LengthBytes = r.StoredFile.LengthBytes
                            })
                            .FirstOrDefault()
                        : null
                    : null,
                SubmittedUtc = x.SubmittedUtc,
                LastStatusChangedUtc = x.LastStatusChangedUtc,
                LastActivityUtc = x.LastActivityUtc,
                IsRejected = x.IsRejected,
                IsWithdrawn = x.IsWithdrawn,
                IsHired = x.IsHired
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<ApplicationSummaryDto>>.Success(new PagedResult<ApplicationSummaryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }
}