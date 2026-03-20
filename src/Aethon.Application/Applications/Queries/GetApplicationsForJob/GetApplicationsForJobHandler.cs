using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Applications.Services;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Applications;
using Aethon.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Applications.Queries.GetApplicationsForJob;

public sealed class GetApplicationsForJobHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ApplicationAccessService _applicationAccessService;

    public GetApplicationsForJobHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        ApplicationAccessService applicationAccessService)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _applicationAccessService = applicationAccessService;
    }

    public async Task<Result<PagedResult<ApplicationSummaryDto>>> HandleAsync(
        GetApplicationsForJobQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<PagedResult<ApplicationSummaryDto>>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var jobExists = await _dbContext.Jobs
            .AsNoTracking()
            .AnyAsync(x => x.Id == query.JobId, cancellationToken);

        if (!jobExists)
        {
            return Result<PagedResult<ApplicationSummaryDto>>.Failure(
                "jobs.not_found",
                "The requested job was not found.");
        }

        var canView = await _applicationAccessService.CanViewJobApplicationsAsync(
            _currentUserAccessor.UserId,
            query.JobId,
            cancellationToken);

        if (!canView)
        {
            return Result<PagedResult<ApplicationSummaryDto>>.Failure(
                "applications.forbidden",
                "The current user cannot view applications for this job.");
        }

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var baseQuery = _dbContext.JobApplications
            .AsNoTracking()
            .Where(x => x.JobId == query.JobId);

        if (query.Status.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Status == query.Status.Value);
        }

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