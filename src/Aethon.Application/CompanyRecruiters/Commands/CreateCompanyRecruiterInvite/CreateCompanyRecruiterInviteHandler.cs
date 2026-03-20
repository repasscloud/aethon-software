using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.CompanyRecruiters;
using Aethon.Shared.Enums;
using Aethon.Shared.RecruiterCompanies;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.CompanyRecruiters.Commands.CreateCompanyRecruiterInvite;

public sealed class CreateCompanyRecruiterInviteHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateCompanyRecruiterInviteHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<RecruiterCompanyRelationshipDto>> HandleAsync(
        CreateCompanyRecruiterInviteDto dto,
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
            return Result<RecruiterCompanyRelationshipDto>.Failure("organisations.not_found", "No active company membership found.");

        var isAdminOrOwner = myMembership.IsOwner ||
                             myMembership.CompanyRole is CompanyRole.Owner or CompanyRole.Admin;

        if (!isAdminOrOwner)
            return Result<RecruiterCompanyRelationshipDto>.Failure("auth.forbidden", "You do not have permission to invite recruiters.");

        var companyOrgId = myMembership.OrganisationId;

        var recruiterOrg = await _db.Organisations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == dto.RecruiterOrganisationId, ct);

        if (recruiterOrg is null)
            return Result<RecruiterCompanyRelationshipDto>.Failure("organisations.not_found", "The recruiter organisation was not found.");

        var utcNow = _dateTimeProvider.UtcNow;

        var scope = OrganisationRecruitmentPartnershipScope.None;
        if (dto.AllowCreateDraftJobs) scope |= OrganisationRecruitmentPartnershipScope.CreateDraftJobs;
        if (dto.AllowSubmitJobsForApproval) scope |= OrganisationRecruitmentPartnershipScope.SubmitJobsForApproval;
        if (dto.AllowManageApprovedJobs) scope |= OrganisationRecruitmentPartnershipScope.ManageApprovedJobs;
        if (dto.AllowViewCandidates) scope |= OrganisationRecruitmentPartnershipScope.ViewCandidates;
        if (dto.AllowSubmitCandidates) scope |= OrganisationRecruitmentPartnershipScope.SubmitCandidates;
        if (dto.AllowCommunicateWithCandidates) scope |= OrganisationRecruitmentPartnershipScope.CommunicateWithCandidates;
        if (dto.AllowScheduleInterviews) scope |= OrganisationRecruitmentPartnershipScope.ScheduleInterviews;
        if (dto.AllowPublishJobs) scope |= OrganisationRecruitmentPartnershipScope.PublishJobs;

        var partnership = new OrganisationRecruitmentPartnership
        {
            Id = Guid.NewGuid(),
            RecruiterOrganisationId = dto.RecruiterOrganisationId,
            CompanyOrganisationId = companyOrgId,
            Status = OrganisationRecruitmentPartnershipStatus.Active,
            Scope = scope,
            RecruiterCanCreateUnclaimedCompanyJobs = dto.RecruiterCanCreateUnclaimedCompanyJobs,
            RecruiterCanPublishJobs = dto.RecruiterCanPublishJobs,
            RecruiterCanManageCandidates = dto.RecruiterCanManageCandidates,
            ApprovedByUserId = currentUserId,
            ApprovedUtc = utcNow,
            Notes = dto.Message,
            CreatedUtc = utcNow,
            CreatedByUserId = currentUserId
        };

        _db.OrganisationRecruitmentPartnerships.Add(partnership);
        await _db.SaveChangesAsync(ct);

        return Result<RecruiterCompanyRelationshipDto>.Success(new RecruiterCompanyRelationshipDto
        {
            Id = partnership.Id,
            RecruiterOrganisationId = partnership.RecruiterOrganisationId,
            CompanyOrganisationId = partnership.CompanyOrganisationId,
            RecruiterOrganisationName = recruiterOrg.Name,
            CompanyOrganisationName = myMembership.Organisation.Name,
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
