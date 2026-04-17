using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.RecruiterCompanies;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.RecruiterCompanies.Commands.CreateRecruiterCompanyRequest;

public sealed class CreateRecruiterCompanyRequestHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateRecruiterCompanyRequestHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<RecruiterCompanyRelationshipDto>> HandleAsync(
        CreateRecruiterCompanyRequestDto request,
        CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<RecruiterCompanyRelationshipDto>.Failure("auth.unauthenticated", "Not authenticated.");

        var currentUserId = _currentUser.UserId;

        var myMembership = await _db.OrganisationMemberships
            .AsNoTracking()
            .Include(m => m.Organisation)
            .Where(m => m.UserId == currentUserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (myMembership is null)
            return Result<RecruiterCompanyRelationshipDto>.Failure("organisations.not_found", "No active recruiter membership found.");

        var recruiterOrgId = myMembership.OrganisationId;

        var companyOrg = await _db.Organisations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.CompanyOrganisationId, ct);

        if (companyOrg is null)
            return Result<RecruiterCompanyRelationshipDto>.Failure("organisations.not_found", "The company organisation was not found.");

        var existing = await _db.OrganisationRecruitmentPartnerships
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.RecruiterOrganisationId == recruiterOrgId &&
                p.CompanyOrganisationId == request.CompanyOrganisationId &&
                p.Status == OrganisationRecruitmentPartnershipStatus.Pending, ct);

        if (existing is not null)
            return Result<RecruiterCompanyRelationshipDto>.Failure("partnerships.already_pending", "A pending request already exists for this company.");

        var utcNow = _dateTimeProvider.UtcNow;

        var partnership = new OrganisationRecruitmentPartnership
        {
            Id = Guid.NewGuid(),
            RecruiterOrganisationId = recruiterOrgId,
            CompanyOrganisationId = request.CompanyOrganisationId,
            Status = OrganisationRecruitmentPartnershipStatus.Pending,
            Scope = OrganisationRecruitmentPartnershipScope.None,
            Notes = request.Message,
            RequestedByUserId = currentUserId,
            CreatedUtc = utcNow,
            CreatedByUserId = currentUserId
        };

        _db.OrganisationRecruitmentPartnerships.Add(partnership);
        await _db.SaveChangesAsync(ct);

        return Result<RecruiterCompanyRelationshipDto>.Success(new RecruiterCompanyRelationshipDto
        {
            Id = partnership.Id,
            RecruiterOrganisationId = recruiterOrgId,
            CompanyOrganisationId = request.CompanyOrganisationId,
            RecruiterOrganisationName = myMembership.Organisation.Name,
            CompanyOrganisationName = companyOrg.Name,
            Status = partnership.Status,
            Scope = partnership.Scope,
            RecruiterCanCreateUnclaimedCompanyJobs = partnership.RecruiterCanCreateUnclaimedCompanyJobs,
            RecruiterCanPublishJobs = partnership.RecruiterCanPublishJobs,
            RecruiterCanManageCandidates = partnership.RecruiterCanManageCandidates,
            RequestedByUserId = partnership.RequestedByUserId,
            ApprovedByUserId = partnership.ApprovedByUserId,
            CreatedUtc = partnership.CreatedUtc,
            ApprovedUtc = partnership.ApprovedUtc,
            Notes = partnership.Notes
        });
    }
}
