using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.RecruiterCompanies;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.CompanyRecruiters.Queries.GetCompanyRecruiters;

public sealed class GetCompanyRecruitersHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public GetCompanyRecruitersHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<RecruiterCompanyRelationshipDto>>> HandleAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<List<RecruiterCompanyRelationshipDto>>.Failure("auth.unauthenticated", "Not authenticated.");

        var myMembership = await _db.OrganisationMemberships
            .AsNoTracking()
            .Include(m => m.Organisation)
            .Where(m => m.UserId == _currentUser.UserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (myMembership is null)
            return Result<List<RecruiterCompanyRelationshipDto>>.Failure("organisations.not_found", "No active company membership found.");

        var companyOrgId = myMembership.OrganisationId;

        var partnerships = await _db.OrganisationRecruitmentPartnerships
            .AsNoTracking()
            .Include(p => p.CompanyOrganisation)
            .Include(p => p.RecruiterOrganisation)
            .Where(p =>
                p.CompanyOrganisationId == companyOrgId &&
                p.Status != OrganisationRecruitmentPartnershipStatus.Revoked &&
                p.Status != OrganisationRecruitmentPartnershipStatus.Rejected)
            .OrderByDescending(p => p.CreatedUtc)
            .ToListAsync(ct);

        var result = partnerships.Select(p => new RecruiterCompanyRelationshipDto
        {
            Id = p.Id,
            RecruiterOrganisationId = p.RecruiterOrganisationId,
            CompanyOrganisationId = p.CompanyOrganisationId,
            RecruiterOrganisationName = p.RecruiterOrganisation.Name,
            CompanyOrganisationName = p.CompanyOrganisation.Name,
            Status = p.Status,
            Scope = p.Scope,
            RecruiterCanCreateUnclaimedCompanyJobs = p.RecruiterCanCreateUnclaimedCompanyJobs,
            RecruiterCanPublishJobs = p.RecruiterCanPublishJobs,
            RecruiterCanManageCandidates = p.RecruiterCanManageCandidates,
            RequestedByUserId = p.RequestedByUserId,
            ApprovedByUserId = p.ApprovedByUserId,
            CreatedUtc = p.CreatedUtc,
            ApprovedUtc = p.ApprovedUtc,
            Notes = p.Notes
        }).ToList();

        return Result<List<RecruiterCompanyRelationshipDto>>.Success(result);
    }
}
