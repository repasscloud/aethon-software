using Aethon.Data;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Services;

public sealed class OrganisationAccessService
{
    private readonly AethonDbContext _dbContext;

    public OrganisationAccessService(AethonDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> IsActiveMemberAsync(
        Guid userId,
        Guid organisationId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.OrganisationMemberships.AnyAsync(
            x => x.UserId == userId &&
                 x.OrganisationId == organisationId &&
                 x.Status == MembershipStatus.Active,
            cancellationToken);
    }

    public Task<bool> CanCreateJobsAsync(
        Guid userId,
        Guid organisationId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.OrganisationMemberships.AnyAsync(
            x => x.UserId == userId &&
                 x.OrganisationId == organisationId &&
                 x.Status == MembershipStatus.Active &&
                 (
                     x.IsOwner ||
                     x.CompanyRole == CompanyRole.Owner ||
                     x.CompanyRole == CompanyRole.Admin ||
                     x.CompanyRole == CompanyRole.Recruiter ||
                     x.RecruiterRole == RecruiterRole.Owner ||
                     x.RecruiterRole == RecruiterRole.Admin ||
                     x.RecruiterRole == RecruiterRole.Recruiter ||
                     x.RecruiterRole == RecruiterRole.TeamLead
                 ),
            cancellationToken);
    }

    public async Task<bool> CanViewJobAsync(
        Guid userId,
        Guid ownedByOrganisationId,
        Guid? managedByOrganisationId,
        CancellationToken cancellationToken = default)
    {
        if (await IsActiveMemberAsync(userId, ownedByOrganisationId, cancellationToken))
        {
            return true;
        }

        return managedByOrganisationId.HasValue &&
               await IsActiveMemberAsync(userId, managedByOrganisationId.Value, cancellationToken);
    }
}
