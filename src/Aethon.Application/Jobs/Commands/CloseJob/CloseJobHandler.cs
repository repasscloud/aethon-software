using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Application.Organisations.Services;
using Aethon.Data;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Jobs.Commands.CloseJob;

public sealed class CloseJobHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly OrganisationAccessService _orgAccess;

    public CloseJobHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        OrganisationAccessService orgAccess)
    {
        _db = db;
        _currentUser = currentUser;
        _orgAccess = orgAccess;
    }

    public async Task<Result> HandleAsync(Guid jobId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure("auth.unauthenticated", "Not authenticated.");

        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);

        if (job is null)
            return Result.Failure("jobs.not_found", "Job not found.");

        var canEdit = await _orgAccess.CanCreateJobsAsync(_currentUser.UserId, job.OwnedByOrganisationId, ct) ||
                      (job.ManagedByOrganisationId.HasValue &&
                       await _orgAccess.CanCreateJobsAsync(_currentUser.UserId, job.ManagedByOrganisationId.Value, ct));

        if (!canEdit)
            return Result.Failure("jobs.forbidden", "Insufficient permissions to close this job.");

        if (job.Status == JobStatus.Closed)
            return Result.Failure("jobs.already_closed", "Job is already closed.");

        var now = DateTime.UtcNow;
        job.Status = JobStatus.Closed;
        job.ClosedUtc = now;
        job.UpdatedUtc = now;
        job.UpdatedByUserId = _currentUser.UserId;

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
