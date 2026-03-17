using Aethon.Data;
using Aethon.Shared.CompanyJobsApproval;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.CompanyJobsApproval;

public sealed class CompanyJobApprovalService : ICompanyJobApprovalQueryService, ICompanyJobApprovalCommandService
{
    private readonly AethonDbContext _db;

    public CompanyJobApprovalService(AethonDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<JobSummaryDto>> GetPendingApprovalsAsync(
        Guid companyUserId,
        CancellationToken cancellationToken)
    {
        var companyOrganisationId = await _db.OrganisationMemberships
            .AsNoTracking()
            .Where(x => x.UserId == companyUserId)
            .Select(x => x.OrganisationId)
            .SingleAsync(cancellationToken);

        return await _db.Jobs
            .AsNoTracking()
            .Where(x =>
                x.OwnedByOrganisationId == companyOrganisationId &&
                x.Status == JobStatus.PendingCompanyApproval)
            .OrderByDescending(x => x.SubmittedForApprovalUtc ?? x.CreatedUtc)
            .Select(x => new JobSummaryDto
            {
                Id = x.Id,
                CompanyOrganisationId = x.OwnedByOrganisationId,
                ManagedByRecruiterOrganisationId = x.ManagedByOrganisationId,
                Title = x.Title,
                OrganisationName = x.OwnedByOrganisation.Name,
                Status = x.Status.ToString(),
                CreatedUtc = x.CreatedUtc,
                SubmittedForApprovalUtc = x.SubmittedForApprovalUtc,
                ApprovedUtc = x.ApprovedUtc,
                PublishedUtc = x.PublishedUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task ApproveAsync(Guid companyUserId, Guid jobId, CancellationToken cancellationToken)
    {
        var entity = await GetOwnedPendingJobAsync(companyUserId, jobId, cancellationToken);
        entity.Status = JobStatus.Approved;
        entity.ApprovedUtc = DateTime.UtcNow;
        entity.StatusReason = null;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectAsync(Guid companyUserId, Guid jobId, RejectJobApprovalDto request, CancellationToken cancellationToken)
    {
        var entity = await GetOwnedPendingJobAsync(companyUserId, jobId, cancellationToken);
        entity.Status = JobStatus.Draft;
        entity.StatusReason = request.Reason?.Trim();
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task PublishAsync(Guid companyUserId, Guid jobId, CancellationToken cancellationToken)
    {
        var entity = await GetOwnedJobAsync(companyUserId, jobId, cancellationToken);

        if (entity.Status != JobStatus.Approved)
        {
            throw new InvalidOperationException("Only approved jobs can be published.");
        }

        entity.Status = JobStatus.Published;
        entity.PublishedUtc = DateTime.UtcNow;
        entity.StatusReason = null;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task PutOnHoldAsync(Guid companyUserId, Guid jobId, HoldJobDto request, CancellationToken cancellationToken)
    {
        var entity = await GetOwnedJobAsync(companyUserId, jobId, cancellationToken);
        entity.Status = JobStatus.OnHold;
        entity.StatusReason = request.Reason?.Trim();
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(Guid companyUserId, Guid jobId, CancelJobDto request, CancellationToken cancellationToken)
    {
        var entity = await GetOwnedJobAsync(companyUserId, jobId, cancellationToken);
        entity.Status = JobStatus.Cancelled;
        entity.StatusReason = request.Reason?.Trim();
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Aethon.Data.Entities.Job> GetOwnedPendingJobAsync(Guid companyUserId, Guid jobId, CancellationToken cancellationToken)
    {
        var entity = await GetOwnedJobAsync(companyUserId, jobId, cancellationToken);

        if (entity.Status != JobStatus.PendingCompanyApproval)
        {
            throw new InvalidOperationException("Job is not pending company approval.");
        }

        return entity;
    }

    private async Task<Aethon.Data.Entities.Job> GetOwnedJobAsync(
        Guid companyUserId,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var companyOrganisationId = await _db.OrganisationMemberships
            .AsNoTracking()
            .Where(x => x.UserId == companyUserId)
            .Select(x => x.OrganisationId)
            .SingleAsync(cancellationToken);

        var entity = await _db.Jobs
            .SingleAsync(x => x.Id == jobId, cancellationToken);

        if (entity.OwnedByOrganisationId != companyOrganisationId)
        {
            throw new InvalidOperationException("Company cannot manage this job.");
        }

        return entity;
    }
}
