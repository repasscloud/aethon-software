using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Jobs.Queries.GetMyOrgJobs;

public sealed class GetMyOrgJobsHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public GetMyOrgJobsHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<JobListItemDto>>> HandleAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<List<JobListItemDto>>.Failure("auth.unauthenticated", "Not authenticated.");

        // Find all organisations the user is an active member of
        var orgIds = await _db.OrganisationMemberships
            .AsNoTracking()
            .Where(m => m.UserId == _currentUser.UserId && m.Status == MembershipStatus.Active)
            .Select(m => m.OrganisationId)
            .ToListAsync(ct);

        if (orgIds.Count == 0)
            return Result<List<JobListItemDto>>.Success([]);

        var jobs = await _db.Jobs
            .AsNoTracking()
            .Where(j => orgIds.Contains(j.OwnedByOrganisationId) ||
                        (j.ManagedByOrganisationId.HasValue && orgIds.Contains(j.ManagedByOrganisationId.Value)))
            .OrderByDescending(j => j.CreatedUtc)
            .Select(j => new JobListItemDto
            {
                Id = j.Id,
                Title = j.Title,
                Department = j.Department,
                LocationText = j.LocationText,
                WorkplaceType = j.WorkplaceType,
                EmploymentType = j.EmploymentType,
                Status = j.Status,
                SalaryFrom = j.SalaryFrom,
                SalaryTo = j.SalaryTo,
                SalaryCurrency = j.SalaryCurrency,
                CreatedUtc = j.CreatedUtc,
                PublishedUtc = j.PublishedUtc
            })
            .ToListAsync(ct);

        return Result<List<JobListItemDto>>.Success(jobs);
    }
}
