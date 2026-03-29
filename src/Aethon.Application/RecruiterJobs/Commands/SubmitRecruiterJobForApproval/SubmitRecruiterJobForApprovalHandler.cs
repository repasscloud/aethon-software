using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.RecruiterJobs.Commands.SubmitRecruiterJobForApproval;

public sealed class SubmitRecruiterJobForApprovalHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SubmitRecruiterJobForApprovalHandler(
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
            return Result.Failure("organisations.not_found", "No active recruiter membership found.");

        var recruiterOrgId = myMembership.OrganisationId;

        var job = await _db.Jobs
            .FirstOrDefaultAsync(j =>
                j.Id == jobId &&
                j.ManagedByOrganisationId == recruiterOrgId, ct);

        if (job is null)
            return Result.Failure("jobs.not_found", "The job was not found.");

        if (job.Status != JobStatus.Draft && job.Status != JobStatus.Approved)
            return Result.Failure("jobs.invalid_status", "Only draft or approved jobs can be submitted for approval.");

        var utcNow = _dateTimeProvider.UtcNow;

        job.Status = JobStatus.PendingCompanyApproval;
        job.SubmittedForApprovalUtc = utcNow;
        job.UpdatedUtc = utcNow;
        job.UpdatedByUserId = currentUserId;

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
