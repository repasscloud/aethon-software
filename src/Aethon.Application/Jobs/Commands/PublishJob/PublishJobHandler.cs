using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Application.Organisations.Services;
using Aethon.Data;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Jobs.Commands.PublishJob;

public sealed class PublishJobHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly OrganisationAccessService _orgAccess;

    public PublishJobHandler(
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
            return Result.Failure("jobs.forbidden", "Insufficient permissions to publish this job.");

        if (job.Status is not (JobStatus.Draft or JobStatus.Approved or JobStatus.OnHold))
            return Result.Failure("jobs.invalid_status", $"Cannot publish a job in '{job.Status}' status.");

        if (job.PostingExpiresUtc.HasValue && job.PostingExpiresUtc.Value <= DateTime.UtcNow)
            return Result.Failure("jobs.posting_expired", "This job posting has expired and cannot be published. Please update the expiry date.");

        var now = DateTime.UtcNow;
        job.Status = JobStatus.Published;
        job.PublishedUtc ??= now;
        job.UpdatedUtc = now;
        job.UpdatedByUserId = _currentUser.UserId;

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
