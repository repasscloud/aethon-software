using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Applications.Services;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Jobs;
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

    public async Task<Result<IReadOnlyList<EmployerJobApplicationListItemDto>>> HandleAsync(
        GetApplicationsForJobQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<IReadOnlyList<EmployerJobApplicationListItemDto>>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var jobExists = await _dbContext.Jobs
            .AsNoTracking()
            .AnyAsync(x => x.Id == query.JobId, cancellationToken);

        if (!jobExists)
        {
            return Result<IReadOnlyList<EmployerJobApplicationListItemDto>>.Failure(
                "jobs.not_found",
                "The requested job was not found.");
        }

        var canView = await _applicationAccessService.CanViewJobApplicationsAsync(
            _currentUserAccessor.UserId,
            query.JobId,
            cancellationToken);

        if (!canView)
        {
            return Result<IReadOnlyList<EmployerJobApplicationListItemDto>>.Failure(
                "applications.forbidden",
                "The current user cannot view applications for this job.");
        }

        var items = await _dbContext.JobApplications
            .AsNoTracking()
            .Where(x => x.JobId == query.JobId)
            .OrderByDescending(x => x.SubmittedUtc)
            .Select(x => new EmployerJobApplicationListItemDto
            {
                Id = x.Id,
                JobId = x.JobId,
                ApplicantUserId = x.UserId.ToString(),
                ApplicantDisplayName = x.User.DisplayName,
                ApplicantEmail = x.User.Email ?? string.Empty,
                Status = x.Status.ToString(),
                Source = x.Source,
                SubmittedUtc = x.SubmittedUtc,
                LastStatusChangedUtc = x.LastStatusChangedUtc
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<EmployerJobApplicationListItemDto>>.Success(items);
    }
}
