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
            .Select(MapRelationship())
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RecruiterCompanyRelationshipDto>> GetRelationshipsAsync(
        Guid companyUserId,
        CancellationToken cancellationToken)
    {
        var companyOrganisationId = await GetCompanyOrganisationIdAsync(companyUserId, cancellationToken);

        return await _db.CompanyRecruiterRelationships
            .AsNoTracking()
            .Where(x => x.CompanyOrganisationId == companyOrganisationId)
            .OrderByDescending(x => x.CreatedUtc)
            .Select(MapRelationship())
            .ToListAsync(cancellationToken);
    }

    public async Task ApproveAsync(
        Guid companyUserId,
        Guid relationshipId,
        ApproveRecruiterCompanyRequestDto request,
        CancellationToken cancellationToken)
    {
        var entity = await GetOwnedRelationshipAsync(companyUserId, relationshipId, cancellationToken);

        if (entity.Status != CompanyRecruiterRelationshipStatus.Pending)
        {
            throw new InvalidOperationException("Only pending recruiter relationships can be approved.");
        }

        entity.Scope = BuildScope(request);
        entity.RecruiterCanCreateUnclaimedCompanyJobs = request.RecruiterCanCreateUnclaimedCompanyJobs;
        entity.RecruiterCanPublishJobs = request.RecruiterCanPublishJobs;
        entity.RecruiterCanManageCandidates = request.RecruiterCanManageCandidates;
        entity.Status = CompanyRecruiterRelationshipStatus.Active;
        entity.ApprovedByUserId = companyUserId;
        entity.ApprovedUtc = DateTime.UtcNow;
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes)
            ? entity.Notes
            : request.Notes.Trim();
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

        if (entity.Status != CompanyRecruiterRelationshipStatus.Active &&
            entity.Status != CompanyRecruiterRelationshipStatus.Pending)
        {
            throw new InvalidOperationException("Only active/pending recruiter relationships can be suspended.");
        }

        entity.Status = CompanyRecruiterRelationshipStatus.Suspended;
        entity.UpdatedUtc = DateTime.UtcNow;
        entity.UpdatedByUserId = companyUserId;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task InviteAsync(
        Guid companyUserId,
        CreateCompanyRecruiterInviteDto request,
        CancellationToken cancellationToken)
    {
        var companyOrganisationId = await GetCompanyOrganisationIdAsync(companyUserId, cancellationToken);

        if (request.RecruiterOrganisationId == Guid.Empty)
        {
            throw new InvalidOperationException("Recruiter organisation is required.");
        }

        if (request.RecruiterOrganisationId == companyOrganisationId)
        {
            throw new InvalidOperationException("Company cannot invite its own organisation.");
        }

        var recruiterExists = await _db.Organisations
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == request.RecruiterOrganisationId && x.Type == OrganisationType.RecruiterAgency,
                cancellationToken);

        if (!recruiterExists)
        {
            throw new InvalidOperationException("Recruiter organisation was not found.");
        }

        var utcNow = DateTime.UtcNow;

        var entity = await _db.CompanyRecruiterRelationships
            .SingleOrDefaultAsync(
                x =>
                    x.CompanyOrganisationId == companyOrganisationId &&
                    x.RecruiterOrganisationId == request.RecruiterOrganisationId,
                cancellationToken);

        if (entity is null)
        {
            entity = new CompanyRecruiterRelationship
            {
                Id = Guid.NewGuid(),
                CompanyOrganisationId = companyOrganisationId,
                RecruiterOrganisationId = request.RecruiterOrganisationId,
                Status = CompanyRecruiterRelationshipStatus.Pending,
                Scope = BuildScope(request),
                RecruiterCanCreateUnclaimedCompanyJobs = request.RecruiterCanCreateUnclaimedCompanyJobs,
                RecruiterCanPublishJobs = request.RecruiterCanPublishJobs,
                RecruiterCanManageCandidates = request.RecruiterCanManageCandidates,
                RequestedByUserId = companyUserId,
                ApprovedByUserId = null,
                ApprovedUtc = null,
                Notes = request.Message?.Trim(),
                CreatedUtc = utcNow,
                CreatedByUserId = companyUserId,
                UpdatedUtc = utcNow,
                UpdatedByUserId = companyUserId
            };

            _db.CompanyRecruiterRelationships.Add(entity);
        }
        else
        {
            if (entity.Status == CompanyRecruiterRelationshipStatus.Pending)
            {
                throw new InvalidOperationException("A pending recruiter-company relationship already exists.");
            }

            if (entity.Status == CompanyRecruiterRelationshipStatus.Active)
            {
                throw new InvalidOperationException("An active recruiter-company relationship already exists.");
            }

            entity.Status = CompanyRecruiterRelationshipStatus.Pending;
            entity.Scope = BuildScope(request);
            entity.RecruiterCanCreateUnclaimedCompanyJobs = request.RecruiterCanCreateUnclaimedCompanyJobs;
            entity.RecruiterCanPublishJobs = request.RecruiterCanPublishJobs;
            entity.RecruiterCanManageCandidates = request.RecruiterCanManageCandidates;
            entity.RequestedByUserId = companyUserId;
            entity.ApprovedByUserId = null;
            entity.ApprovedUtc = null;
            entity.Notes = request.Message?.Trim();
            entity.UpdatedUtc = utcNow;
            entity.UpdatedByUserId = companyUserId;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static System.Linq.Expressions.Expression<Func<CompanyRecruiterRelationship, RecruiterCompanyRelationshipDto>> MapRelationship()
    {
        return x => new RecruiterCompanyRelationshipDto
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
        };
    }

    private static CompanyRecruiterRelationshipScope BuildScope(ApproveRecruiterCompanyRequestDto request)
    {
        var scope = CompanyRecruiterRelationshipScope.None;

        if (request.AllowCreateDraftJobs)
        {
            scope |= CompanyRecruiterRelationshipScope.CreateDraftJobs;
        }

        if (request.AllowSubmitJobsForApproval)
        {
            scope |= CompanyRecruiterRelationshipScope.SubmitJobsForApproval;
        }

        if (request.AllowManageApprovedJobs)
        {
            scope |= CompanyRecruiterRelationshipScope.ManageApprovedJobs;
        }

        if (request.AllowViewCandidates)
        {
            scope |= CompanyRecruiterRelationshipScope.ViewCandidates;
        }

        if (request.AllowSubmitCandidates)
        {
            scope |= CompanyRecruiterRelationshipScope.SubmitCandidates;
        }

        if (request.AllowCommunicateWithCandidates)
        {
            scope |= CompanyRecruiterRelationshipScope.CommunicateWithCandidates;
        }

        if (request.AllowScheduleInterviews)
        {
            scope |= CompanyRecruiterRelationshipScope.ScheduleInterviews;
        }

        if (request.AllowPublishJobs)
        {
            scope |= CompanyRecruiterRelationshipScope.PublishJobs;
        }

        return scope;
    }

    private static CompanyRecruiterRelationshipScope BuildScope(CreateCompanyRecruiterInviteDto request)
    {
        var scope = CompanyRecruiterRelationshipScope.None;

        if (request.AllowCreateDraftJobs)
        {
            scope |= CompanyRecruiterRelationshipScope.CreateDraftJobs;
        }

        if (request.AllowSubmitJobsForApproval)
        {
            scope |= CompanyRecruiterRelationshipScope.SubmitJobsForApproval;
        }

        if (request.AllowManageApprovedJobs)
        {
            scope |= CompanyRecruiterRelationshipScope.ManageApprovedJobs;
        }

        if (request.AllowViewCandidates)
        {
            scope |= CompanyRecruiterRelationshipScope.ViewCandidates;
        }

        if (request.AllowSubmitCandidates)
        {
            scope |= CompanyRecruiterRelationshipScope.SubmitCandidates;
        }

        if (request.AllowCommunicateWithCandidates)
        {
            scope |= CompanyRecruiterRelationshipScope.CommunicateWithCandidates;
        }

        if (request.AllowScheduleInterviews)
        {
            scope |= CompanyRecruiterRelationshipScope.ScheduleInterviews;
        }

        if (request.AllowPublishJobs)
        {
            scope |= CompanyRecruiterRelationshipScope.PublishJobs;
        }

        return scope;
    }

    private async Task<Guid> GetCompanyOrganisationIdAsync(
        Guid companyUserId,
        CancellationToken cancellationToken)
    {
        return await _db.OrganisationMemberships
            .AsNoTracking()
            .Where(x =>
                x.UserId == companyUserId &&
                x.Organisation.Type == OrganisationType.Company)
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
