using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.CompanyRecruiters;
using Aethon.Shared.Enums;
using Aethon.Shared.RecruiterCompanies;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.CompanyRecruiters;

public sealed class CompanyRecruiterService
    : ICompanyRecruiterQueryService, ICompanyRecruiterCommandService
{
    private readonly AethonDbContext _db;

    public CompanyRecruiterService(AethonDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RecruiterCompanyRelationshipDto>> GetPendingRequestsAsync(
        Guid companyUserId,
        CancellationToken cancellationToken)
    {
        var companyOrganisationId = await GetCompanyOrganisationIdAsync(companyUserId, cancellationToken);

        return await _db.CompanyRecruiterRelationships
            .AsNoTracking()
            .Where(x =>
                x.CompanyOrganisationId == companyOrganisationId &&
                x.Status == CompanyRecruiterRelationshipStatus.Pending)
            .OrderByDescending(x => x.CreatedUtc)
            .Select(x => new RecruiterCompanyRelationshipDto
            {
                Id = x.Id,
                CompanyOrganisationId = x.CompanyOrganisationId,
                CompanyOrganisationName = x.CompanyOrganisation.Name,
                RecruiterOrganisationId = x.RecruiterOrganisationId,
                RecruiterOrganisationName = x.RecruiterOrganisation.Name,
                Status = x.Status,
                Scope = x.Scope,
                RecruiterCanCreateUnclaimedCompanyJobs = x.RecruiterCanCreateUnclaimedCompanyJobs,
                RecruiterCanPublishJobs = x.RecruiterCanPublishJobs,
                RecruiterCanManageCandidates = x.RecruiterCanManageCandidates,
                RequestedByUserId = x.RequestedByUserId,
                ApprovedByUserId = x.ApprovedByUserId,
                CreatedUtc = x.CreatedUtc,
                ApprovedUtc = x.ApprovedUtc,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task ApproveAsync(
        Guid companyUserId,
        Guid relationshipId,
        CancellationToken cancellationToken)
    {
        var entity = await GetOwnedRelationshipAsync(companyUserId, relationshipId, cancellationToken);

        if (entity.Status != CompanyRecruiterRelationshipStatus.Pending)
        {
            throw new InvalidOperationException("Only pending recruiter relationships can be approved.");
        }

        entity.Status = CompanyRecruiterRelationshipStatus.Active;
        entity.ApprovedByUserId = companyUserId;
        entity.ApprovedUtc = DateTime.UtcNow;
        entity.UpdatedUtc = DateTime.UtcNow;
        entity.UpdatedByUserId = companyUserId;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectAsync(
        Guid companyUserId,
        Guid relationshipId,
        RejectRecruiterCompanyRequestDto request,
        CancellationToken cancellationToken)
    {
        var entity = await GetOwnedRelationshipAsync(companyUserId, relationshipId, cancellationToken);

        if (entity.Status != CompanyRecruiterRelationshipStatus.Pending)
        {
            throw new InvalidOperationException("Only pending recruiter relationships can be rejected.");
        }

        entity.Status = CompanyRecruiterRelationshipStatus.Rejected;
        entity.Notes = string.IsNullOrWhiteSpace(request.Reason)
            ? "Rejected by company."
            : request.Reason.Trim();
        entity.UpdatedUtc = DateTime.UtcNow;
        entity.UpdatedByUserId = companyUserId;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task SuspendAsync(
        Guid companyUserId,
        Guid relationshipId,
        CancellationToken cancellationToken)
    {
        var entity = await GetOwnedRelationshipAsync(companyUserId, relationshipId, cancellationToken);

        if (entity.Status != CompanyRecruiterRelationshipStatus.Active ||
            entity.Status != CompanyRecruiterRelationshipStatus.Pending)
        {
            throw new InvalidOperationException("Only active/pending recruiter relationships can be suspended.");
        }

        entity.Status = CompanyRecruiterRelationshipStatus.Suspended;
        entity.UpdatedUtc = DateTime.UtcNow;
        entity.UpdatedByUserId = companyUserId;

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid> GetCompanyOrganisationIdAsync(
        Guid companyUserId,
        CancellationToken cancellationToken)
    {
        return await _db.OrganisationMemberships
            .AsNoTracking()
            .Where(x => x.UserId == companyUserId)
            .Select(x => x.OrganisationId)
            .SingleAsync(cancellationToken);
    }

    private async Task<CompanyRecruiterRelationship> GetOwnedRelationshipAsync(
        Guid companyUserId,
        Guid relationshipId,
        CancellationToken cancellationToken)
    {
        var companyOrganisationId = await GetCompanyOrganisationIdAsync(companyUserId, cancellationToken);

        var entity = await _db.CompanyRecruiterRelationships
            .SingleAsync(x => x.Id == relationshipId, cancellationToken);

        if (entity.CompanyOrganisationId != companyOrganisationId)
        {
            throw new InvalidOperationException("Company cannot manage this recruiter relationship.");
        }

        return entity;
    }
}