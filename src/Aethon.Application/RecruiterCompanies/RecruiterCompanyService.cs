using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.RecruiterCompanies;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.RecruiterCompanies;

public sealed class RecruiterCompanyService
    : IRecruiterCompanyQueryService, IRecruiterCompanyCommandService
{
    private readonly AethonDbContext _db;

    public RecruiterCompanyService(AethonDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RecruiterCompanyRelationshipDto>> GetRecruiterRelationshipsAsync(
        Guid recruiterUserId,
        CancellationToken cancellationToken)
    {
        var recruiterOrganisationId = await GetRecruiterOrganisationIdAsync(recruiterUserId, cancellationToken);

        return await _db.CompanyRecruiterRelationships
            .AsNoTracking()
            .Where(x => x.RecruiterOrganisationId == recruiterOrganisationId)
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

    public async Task<RecruiterCompanyRelationshipDto> CreateRequestAsync(
        Guid recruiterUserId,
        CreateRecruiterCompanyRequestDto request,
        CancellationToken cancellationToken)
    {
        var recruiterOrganisationId = await GetRecruiterOrganisationIdAsync(recruiterUserId, cancellationToken);

        if (request.CompanyOrganisationId == Guid.Empty)
        {
            throw new InvalidOperationException("Company organisation is required.");
        }

        if (request.CompanyOrganisationId == recruiterOrganisationId)
        {
            throw new InvalidOperationException("Recruiter cannot request a relationship with its own organisation.");
        }

        var companyExists = await _db.Organisations
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == request.CompanyOrganisationId && x.Type == OrganisationType.Company,
                cancellationToken);

        if (!companyExists)
        {
            throw new InvalidOperationException("Company organisation was not found.");
        }

        var utcNow = DateTime.UtcNow;

        var entity = await _db.CompanyRecruiterRelationships
            .SingleOrDefaultAsync(
                x =>
                    x.CompanyOrganisationId == request.CompanyOrganisationId &&
                    x.RecruiterOrganisationId == recruiterOrganisationId,
                cancellationToken);

        if (entity is null)
        {
            entity = new CompanyRecruiterRelationship
            {
                Id = Guid.NewGuid(),
                CompanyOrganisationId = request.CompanyOrganisationId,
                RecruiterOrganisationId = recruiterOrganisationId,
                Status = CompanyRecruiterRelationshipStatus.Pending,
                Scope = CompanyRecruiterRelationshipScope.None,
                RecruiterCanCreateUnclaimedCompanyJobs = false,
                RecruiterCanPublishJobs = false,
                RecruiterCanManageCandidates = false,
                RequestedByUserId = recruiterUserId,
                ApprovedByUserId = null,
                ApprovedUtc = null,
                Notes = request.Message?.Trim(),
                CreatedUtc = utcNow,
                CreatedByUserId = recruiterUserId,
                UpdatedUtc = utcNow,
                UpdatedByUserId = recruiterUserId
            };

            _db.CompanyRecruiterRelationships.Add(entity);
        }
        else
        {
            if (entity.Status == CompanyRecruiterRelationshipStatus.Pending)
            {
                throw new InvalidOperationException("A pending recruiter-company request already exists.");
            }

            if (entity.Status == CompanyRecruiterRelationshipStatus.Active)
            {
                throw new InvalidOperationException("An active recruiter-company relationship already exists.");
            }

            entity.Status = CompanyRecruiterRelationshipStatus.Pending;
            entity.Scope = CompanyRecruiterRelationshipScope.None;
            entity.RecruiterCanCreateUnclaimedCompanyJobs = false;
            entity.RecruiterCanPublishJobs = false;
            entity.RecruiterCanManageCandidates = false;
            entity.RequestedByUserId = recruiterUserId;
            entity.ApprovedByUserId = null;
            entity.ApprovedUtc = null;
            entity.Notes = request.Message?.Trim();
            entity.UpdatedUtc = utcNow;
            entity.UpdatedByUserId = recruiterUserId;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return await MapRelationshipAsync(entity.Id, cancellationToken);
    }

    public async Task CancelRequestAsync(
        Guid recruiterUserId,
        Guid relationshipId,
        CancellationToken cancellationToken)
    {
        var entity = await GetOwnedRelationshipAsync(recruiterUserId, relationshipId, cancellationToken);

        if (entity.Status != CompanyRecruiterRelationshipStatus.Pending)
        {
            throw new InvalidOperationException("Only pending recruiter-company requests can be cancelled.");
        }

        entity.Status = CompanyRecruiterRelationshipStatus.Revoked;
        entity.UpdatedUtc = DateTime.UtcNow;
        entity.UpdatedByUserId = recruiterUserId;

        await _db.SaveChangesAsync(cancellationToken);
    }

    private Task<RecruiterCompanyRelationshipDto> MapRelationshipAsync(
        Guid relationshipId,
        CancellationToken cancellationToken)
    {
        return _db.CompanyRecruiterRelationships
            .AsNoTracking()
            .Where(x => x.Id == relationshipId)
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
            .SingleAsync(cancellationToken);
    }

    private async Task<Guid> GetRecruiterOrganisationIdAsync(
        Guid recruiterUserId,
        CancellationToken cancellationToken)
    {
        return await _db.OrganisationMemberships
            .AsNoTracking()
            .Where(x =>
                x.UserId == recruiterUserId &&
                x.Organisation.Type == OrganisationType.RecruiterAgency)
            .Select(x => x.OrganisationId)
            .SingleAsync(cancellationToken);
    }

    private async Task<CompanyRecruiterRelationship> GetOwnedRelationshipAsync(
        Guid recruiterUserId,
        Guid relationshipId,
        CancellationToken cancellationToken)
    {
        var recruiterOrganisationId = await GetRecruiterOrganisationIdAsync(recruiterUserId, cancellationToken);

        var entity = await _db.CompanyRecruiterRelationships
            .SingleAsync(x => x.Id == relationshipId, cancellationToken);

        if (entity.RecruiterOrganisationId != recruiterOrganisationId)
        {
            throw new InvalidOperationException("Recruiter cannot manage this company relationship.");
        }

        return entity;
    }
}