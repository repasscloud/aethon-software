using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Caching;
using Aethon.Application.Common.Caching;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Applications;
using Aethon.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Applications.Queries.GetMyApplications;

public sealed class GetMyApplicationsHandler
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(2);

    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAppCache _cache;

    public GetMyApplicationsHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IAppCache cache)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _cache = cache;
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
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);
        var currentUserId = _currentUserAccessor.UserId;

        var cacheKey = CacheKeys.MyApplications(currentUserId, page, pageSize);

        var result = await _cache.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                var baseQuery = _dbContext.JobApplications
                    .AsNoTracking()
                    .Where(x => x.UserId == currentUserId);

                var totalCount = await baseQuery.CountAsync(ct);

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
                        SubmittedUtc = x.SubmittedUtc,
                        LastStatusChangedUtc = x.LastStatusChangedUtc,
                        LastActivityUtc = x.LastActivityUtc,
                        IsRejected = x.IsRejected,
                        IsWithdrawn = x.IsWithdrawn,
                        IsHired = x.IsHired
                    })
                    .ToListAsync(ct);

                return new PagedResult<ApplicationSummaryDto>
                {
                    Items = items,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            },
            CacheTtl,
            cancellationToken);

        return Result<PagedResult<ApplicationSummaryDto>>.Success(result);
    }
}
