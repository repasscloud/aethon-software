using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.RecruiterJobs.Queries.GetRecruiterJobs;

public sealed class GetRecruiterJobsHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public GetRecruiterJobsHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<JobSummaryDto>>> HandleAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<List<JobSummaryDto>>.Failure("auth.unauthenticated", "Not authenticated.");

        var myMembership = await _db.OrganisationMemberships
            .AsNoTracking()
            .Where(m => m.UserId == _currentUser.UserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (myMembership is null)
            return Result<List<JobSummaryDto>>.Failure("organisations.not_found", "No active recruiter membership found.");

        var recruiterOrgId = myMembership.OrganisationId;

        var jobs = await _db.Jobs
            .AsNoTracking()
            .Include(j => j.OwnedByOrganisation)
            .Where(j => j.ManagedByOrganisationId == recruiterOrgId)
            .OrderByDescending(j => j.CreatedUtc)
            .ToListAsync(ct);

        var result = jobs.Select(j => new JobSummaryDto
        {
            Id = j.Id,
            CompanyOrganisationId = j.OwnedByOrganisationId,
            ManagedByRecruiterOrganisationId = j.ManagedByOrganisationId,
            Title = j.Title,
            OrganisationName = j.OwnedByOrganisation.Name,
            Status = j.Status.ToString(),
            CreatedUtc = j.CreatedUtc,
            SubmittedForApprovalUtc = j.SubmittedForApprovalUtc,
            ApprovedUtc = j.ApprovedUtc,
            PublishedUtc = j.PublishedUtc
        }).ToList();

        return Result<List<JobSummaryDto>>.Success(result);
    }
}
