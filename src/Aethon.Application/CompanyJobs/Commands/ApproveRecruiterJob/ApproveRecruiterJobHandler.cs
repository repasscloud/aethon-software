using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.CompanyJobs.Commands.ApproveRecruiterJob;

public sealed class ApproveRecruiterJobHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ApproveRecruiterJobHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> HandleAsync(Guid jobId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure("auth.unauthenticated", "Not authenticated.");

        var currentUserId = _currentUser.UserId;

        var myMembership = await _db.OrganisationMemberships
            .AsNoTracking()
            .Where(m => m.UserId == currentUserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (myMembership is null)
            return Result.Failure("organisations.not_found", "No active company membership found.");

        var isAdminOrOwner = myMembership.IsOwner ||
                             myMembership.CompanyRole is CompanyRole.Owner or CompanyRole.Admin;

        if (!isAdminOrOwner)
            return Result.Failure("auth.forbidden", "You do not have permission to approve jobs.");

        var companyOrgId = myMembership.OrganisationId;

        var job = await _db.Jobs
            .FirstOrDefaultAsync(j =>
                j.Id == jobId &&
                j.OwnedByOrganisationId == companyOrgId &&
                j.Status == JobStatus.PendingCompanyApproval, ct);

        if (job is null)
            return Result.Failure("jobs.not_found", "The job was not found or is not pending approval.");

        var utcNow = _dateTimeProvider.UtcNow;

        job.Status = JobStatus.Approved;
        job.ApprovedByUserId = currentUserId;
        job.ApprovedUtc = utcNow;
        job.UpdatedUtc = utcNow;
        job.UpdatedByUserId = currentUserId;

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
