using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.RecruiterJobs;

public sealed class RecruiterJobService : IRecruiterJobQueryService, IRecruiterJobCommandService
{
    private readonly AethonDbContext _db;

    public RecruiterJobService(AethonDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<JobSummaryDto>> GetRecruiterJobsAsync(
        Guid recruiterUserId,
        CancellationToken cancellationToken)
    {
        var recruiterOrganisationId = await GetRecruiterOrganisationIdAsync(recruiterUserId, cancellationToken);

        return await _db.Jobs
            .AsNoTracking()
            .Where(x => x.ManagedByOrganisationId == recruiterOrganisationId)
            .OrderByDescending(x => x.CreatedUtc)
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

    public async Task<JobDetailDto> CreateDraftAsync(
        Guid recruiterUserId,
        RecruiterCreateJobDraftDto request,
        CancellationToken cancellationToken)
    {
        var recruiterOrganisationId = await GetRecruiterOrganisationIdAsync(recruiterUserId, cancellationToken);

        var isRecruiterOwnedJob = request.CompanyOrganisationId == recruiterOrganisationId;

        Guid? companyRecruiterRelationshipId = null;
        var relationshipApproved = isRecruiterOwnedJob;

        if (!isRecruiterOwnedJob)
        {
            var relationship = await _db.CompanyRecruiterRelationships
                .AsNoTracking()
                .Where(x =>
                    x.CompanyOrganisationId == request.CompanyOrganisationId &&
                    x.RecruiterOrganisationId == recruiterOrganisationId)
                .Select(x => new
                {
                    x.Id,
                    x.Status
                })
                .SingleOrDefaultAsync(cancellationToken);

            companyRecruiterRelationshipId = relationship?.Id;
            relationshipApproved = relationship?.Status == CompanyRecruiterRelationshipStatus.Active;
        }

        RecruiterJobRules.EnsureRecruiterCanManageCompany(relationshipApproved, relationshipApproved);

        var entity = new Job
        {
            Id = Guid.NewGuid(),
            OwnedByOrganisationId = request.CompanyOrganisationId,
            ManagedByOrganisationId = recruiterOrganisationId,
            ManagedByUserId = recruiterUserId,
            CompanyRecruiterRelationshipId = companyRecruiterRelationshipId,
            CreatedByIdentityUserId = recruiterUserId,
            CreatedByUserId = recruiterUserId,
            Title = request.Title.Trim(),
            Summary = request.Summary?.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            LocationText = request.Location?.Trim(),
            SalaryFrom = request.SalaryMin,
            SalaryTo = request.SalaryMax,
            Status = JobStatus.Draft,
            CreatedUtc = DateTime.UtcNow
        };

        _db.Jobs.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return await MapDetailAsync(entity.Id, cancellationToken);
    }

    public async Task<JobDetailDto> UpdateDraftAsync(
        Guid recruiterUserId,
        Guid jobId,
        RecruiterUpdateJobDraftDto request,
        CancellationToken cancellationToken)
    {
        var entity = await GetManagedJobAsync(recruiterUserId, jobId, cancellationToken);

        RecruiterJobRules.EnsureDraftEditable(entity.Status.ToString());

        entity.Title = request.Title.Trim();
        entity.Summary = request.Summary?.Trim();
        entity.Description = request.Description?.Trim() ?? string.Empty;
        entity.LocationText = request.Location?.Trim();
        entity.SalaryFrom = request.SalaryMin;
        entity.SalaryTo = request.SalaryMax;
        entity.UpdatedUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return await MapDetailAsync(entity.Id, cancellationToken);
    }

    public async Task SubmitForApprovalAsync(
        Guid recruiterUserId,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var entity = await GetManagedJobAsync(recruiterUserId, jobId, cancellationToken);

        if (entity.Status != JobStatus.Draft)
        {
            throw new InvalidOperationException("Only draft jobs can be submitted.");
        }

        entity.Status = JobStatus.PendingCompanyApproval;
        entity.SubmittedForApprovalUtc = DateTime.UtcNow;
        entity.StatusReason = null;
        entity.UpdatedUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task WithdrawAsync(
        Guid recruiterUserId,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var entity = await GetManagedJobAsync(recruiterUserId, jobId, cancellationToken);

        if (entity.Status != JobStatus.PendingCompanyApproval)
        {
            throw new InvalidOperationException("Only pending approval jobs can be withdrawn.");
        }

        entity.Status = JobStatus.Draft;
        entity.StatusReason = "Withdrawn by recruiter";
        entity.SubmittedForApprovalUtc = null;
        entity.UpdatedUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }

    private Task<JobDetailDto> MapDetailAsync(Guid jobId, CancellationToken cancellationToken)
    {
        return _db.Jobs
            .AsNoTracking()
            .Where(x => x.Id == jobId)
            .Select(x => new JobDetailDto
            {
                Id = x.Id,
                CompanyOrganisationId = x.OwnedByOrganisationId,
                ManagedByRecruiterOrganisationId = x.ManagedByOrganisationId,
                Title = x.Title,
                Summary = x.Summary,
                Description = x.Description,
                Department = x.Department,
                Location = x.LocationText,
                WorkplaceType = x.WorkplaceType,
                EmploymentType = x.EmploymentType,
                Requirements = x.Requirements,
                Benefits = x.Benefits,
                SalaryMin = x.SalaryFrom,
                SalaryMax = x.SalaryTo,
                SalaryCurrency = x.SalaryCurrency,
                Status = x.Status,
                StatusReason = x.StatusReason,
                CreatedUtc = x.CreatedUtc,
                SubmittedForApprovalUtc = x.SubmittedForApprovalUtc,
                ApprovedUtc = x.ApprovedUtc,
                PublishedUtc = x.PublishedUtc,
                ClosedUtc = x.ClosedUtc
            })
            .SingleAsync(cancellationToken);
    }

    private async Task<Guid> GetRecruiterOrganisationIdAsync(Guid recruiterUserId, CancellationToken cancellationToken)
    {
        return await _db.OrganisationMemberships
            .AsNoTracking()
            .Where(x => x.UserId == recruiterUserId)
            .Select(x => x.OrganisationId)
            .SingleAsync(cancellationToken);
    }

    private async Task<Job> GetManagedJobAsync(Guid recruiterUserId, Guid jobId, CancellationToken cancellationToken)
    {
        var recruiterOrganisationId = await GetRecruiterOrganisationIdAsync(recruiterUserId, cancellationToken);

        var entity = await _db.Jobs
            .SingleAsync(x => x.Id == jobId, cancellationToken);

        if (entity.ManagedByOrganisationId != recruiterOrganisationId &&
            entity.ManagedByUserId != recruiterUserId &&
            entity.CreatedByIdentityUserId != recruiterUserId)
        {
            throw new InvalidOperationException("Recruiter cannot manage this job.");
        }

        return entity;
    }
}