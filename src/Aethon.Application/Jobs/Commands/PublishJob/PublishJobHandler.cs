using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Caching;
using Aethon.Application.Common.Caching;
using Aethon.Application.Common.Results;
using Aethon.Application.Organisations.Services;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Jobs.Commands.PublishJob;

public sealed class PublishJobHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly OrganisationAccessService _orgAccess;
    private readonly IAppCache _cache;

    public PublishJobHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        OrganisationAccessService orgAccess,
        IAppCache cache)
    {
        _db = db;
        _currentUser = currentUser;
        _orgAccess = orgAccess;
        _cache = cache;
    }

    public async Task<Result> HandleAsync(Guid jobId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure("auth.unauthenticated", "Not authenticated.");

        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);

        if (job is null)
            return Result.Failure("jobs.not_found", "Job not found.");

        var canEdit = _currentUser.IsStaff ||
                      await _orgAccess.CanCreateJobsAsync(_currentUser.UserId, job.OwnedByOrganisationId, ct) ||
                      (job.ManagedByOrganisationId.HasValue &&
                       await _orgAccess.CanCreateJobsAsync(_currentUser.UserId, job.ManagedByOrganisationId.Value, ct));

        if (!canEdit)
            return Result.Failure("jobs.forbidden", "Insufficient permissions to publish this job.");

        if (job.Status is not (JobStatus.Draft or JobStatus.Approved or JobStatus.OnHold))
            return Result.Failure("jobs.invalid_status", $"Cannot publish a job in '{job.Status}' status.");

        var now = DateTime.UtcNow;

        // Server-side expiry cap: Standard ≤ 30 days, Premium ≤ 60 days from now
        var maxExpiry = job.PostingTier == JobPostingTier.Premium
            ? now.AddDays(60)
            : now.AddDays(30);

        if (!job.PostingExpiresUtc.HasValue || job.PostingExpiresUtc.Value > maxExpiry)
            job.PostingExpiresUtc = maxExpiry;

        if (job.PostingExpiresUtc.Value <= now)
            return Result.Failure("jobs.posting_expired", "This job posting has expired and cannot be published. Please update the expiry date.");

        // Consume 1 job posting credit matching the tier
        var requiredCreditType = job.PostingTier == JobPostingTier.Premium
            ? CreditType.JobPostingPremium
            : CreditType.JobPostingStandard;

        var postingCredit = await _db.OrganisationJobCredits
            .Where(c =>
                c.OrganisationId == job.OwnedByOrganisationId &&
                c.CreditType == requiredCreditType &&
                c.QuantityRemaining > 0 &&
                (c.ExpiresAt == null || c.ExpiresAt > now))
            .OrderBy(c => c.ExpiresAt) // consume soonest-expiring first
            .FirstOrDefaultAsync(ct);

        if (postingCredit is null)
            return Result.Failure("billing.no_credits",
                $"No {requiredCreditType} credits available. Purchase credits or add a payment method.");

        postingCredit.QuantityRemaining--;
        _db.CreditConsumptionLogs.Add(new CreditConsumptionLog
        {
            Id = Guid.NewGuid(),
            OrganisationJobCreditId = postingCredit.Id,
            OrganisationId = job.OwnedByOrganisationId,
            JobId = jobId,
            ConsumedByUserId = _currentUser.UserId,
            QuantityConsumed = 1,
            ConsumedAt = now,
            CreatedUtc = now,
            CreatedByUserId = _currentUser.UserId
        });

        // Consume sticky credit if requested; gracefully clear StickyUntilUtc if none available
        if (job.StickyUntilUtc.HasValue && job.StickyUntilUtc.Value > now)
        {
            var stickyType = ResolveStickyType(job.StickyUntilUtc.Value - now);
            var stickyCredit = await _db.OrganisationJobCredits
                .Where(c =>
                    c.OrganisationId == job.OwnedByOrganisationId &&
                    c.CreditType == stickyType &&
                    c.QuantityRemaining > 0 &&
                    (c.ExpiresAt == null || c.ExpiresAt > now))
                .OrderBy(c => c.ExpiresAt)
                .FirstOrDefaultAsync(ct);

            if (stickyCredit is not null)
            {
                stickyCredit.QuantityRemaining--;
                _db.CreditConsumptionLogs.Add(new CreditConsumptionLog
                {
                    Id = Guid.NewGuid(),
                    OrganisationJobCreditId = stickyCredit.Id,
                    OrganisationId = job.OwnedByOrganisationId,
                    JobId = jobId,
                    ConsumedByUserId = _currentUser.UserId,
                    QuantityConsumed = 1,
                    ConsumedAt = now,
                    CreatedUtc = now,
                    CreatedByUserId = _currentUser.UserId
                });
            }
            else
            {
                // No sticky credit — clear to avoid misrepresenting the listing
                job.StickyUntilUtc = null;
            }
        }

        job.Status = JobStatus.Published;
        job.PublishedUtc ??= now;
        job.UpdatedUtc = now;
        job.UpdatedByUserId = _currentUser.UserId;

        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.JobDetail(jobId), ct);

        return Result.Success();
    }

    private static CreditType ResolveStickyType(TimeSpan duration) => duration.TotalDays switch
    {
        <= 1.5 => CreditType.StickyTop24h,
        <= 8.0 => CreditType.StickyTop7d,
        _      => CreditType.StickyTop30d
    };
}
