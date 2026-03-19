using Aethon.Data;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Applications.Services;

public sealed class ApplicationAccessService
{
    private readonly AethonDbContext _db;

    public ApplicationAccessService(AethonDbContext db)
    {
        _db = db;
    }

    public async Task<bool> CanViewJobApplicationsAsync(
        Guid userId,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var access = await _db.Jobs
            .AsNoTracking()
            .Where(x => x.Id == jobId)
            .Select(x => new
            {
                x.Id,
                x.OwnedByOrganisationId,
                x.ManagedByOrganisationId,
                x.OrganisationRecruitmentPartnershipId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (access is null)
        {
            return false;
        }

        return await CanAccessJobApplicationsInternalAsync(
            userId,
            access.OwnedByOrganisationId,
            access.ManagedByOrganisationId,
            access.OrganisationRecruitmentPartnershipId,
            cancellationToken);
    }

    public async Task<bool> CanManageApplicationAsync(
        Guid userId,
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var access = await _db.JobApplications
            .AsNoTracking()
            .Where(x => x.Id == applicationId)
            .Select(x => new
            {
                x.Id,
                x.Job.OwnedByOrganisationId,
                x.Job.ManagedByOrganisationId,
                x.Job.OrganisationRecruitmentPartnershipId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (access is null)
        {
            return false;
        }

        return await CanAccessJobApplicationsInternalAsync(
            userId,
            access.OwnedByOrganisationId,
            access.ManagedByOrganisationId,
            access.OrganisationRecruitmentPartnershipId,
            cancellationToken);
    }

    public async Task<bool> CanViewOwnApplicationAsync(
        Guid userId,
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        return await _db.JobApplications
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == applicationId && x.UserId == userId,
                cancellationToken);
    }

    private async Task<bool> CanAccessJobApplicationsInternalAsync(
        Guid userId,
        Guid ownedByOrganisationId,
        Guid? managedByOrganisationId,
        Guid? partnershipId,
        CancellationToken cancellationToken)
    {
        var organisationIds = new List<Guid> { ownedByOrganisationId };

        if (managedByOrganisationId.HasValue)
        {
            organisationIds.Add(managedByOrganisationId.Value);
        }

        var isActiveMember = await _db.OrganisationMemberships
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId &&
                     x.Status == MembershipStatus.Active &&
                     organisationIds.Contains(x.OrganisationId),
                cancellationToken);

        if (!isActiveMember)
        {
            return false;
        }

        if (!managedByOrganisationId.HasValue || partnershipId is null)
        {
            return true;
        }

        if (managedByOrganisationId.Value == ownedByOrganisationId)
        {
            return true;
        }

        var partnership = await _db.OrganisationRecruitmentPartnerships
            .AsNoTracking()
            .Where(x => x.Id == partnershipId.Value)
            .Select(x => new
            {
                x.Status,
                x.RecruiterCanManageCandidates
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (partnership is null)
        {
            return false;
        }

        if (partnership.Status != OrganisationRecruitmentPartnershipStatus.Active)
        {
            return false;
        }

        return partnership.RecruiterCanManageCandidates;
    }
}
