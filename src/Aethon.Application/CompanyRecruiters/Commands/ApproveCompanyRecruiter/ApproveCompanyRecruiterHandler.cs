using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.CompanyRecruiters;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.CompanyRecruiters.Commands.ApproveCompanyRecruiter;

public sealed class ApproveCompanyRecruiterHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ApproveCompanyRecruiterHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> HandleAsync(
        Guid partnershipId,
        ApproveRecruiterCompanyRequestDto dto,
        CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure("auth.unauthenticated", "Not authenticated.");

        var currentUserId = _currentUser.UserId;

        var myMembership = await _db.OrganisationMemberships
            .AsNoTracking()
            .Where(m => m.UserId == currentUserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (myMembership is null)
            return Result.Failure("organisations.not_found", "No active company membership found.");

        var isAdminOrOwner = myMembership.IsOwner ||
                             myMembership.CompanyRole is CompanyRole.Owner or CompanyRole.Admin;

        if (!isAdminOrOwner)
            return Result.Failure("auth.forbidden", "You do not have permission to approve recruiter requests.");

        var companyOrgId = myMembership.OrganisationId;

        var partnership = await _db.OrganisationRecruitmentPartnerships
            .FirstOrDefaultAsync(p => p.Id == partnershipId && p.CompanyOrganisationId == companyOrgId, ct);

        if (partnership is null)
            return Result.Failure("partnerships.not_found", "The partnership request was not found.");

        if (partnership.Status != OrganisationRecruitmentPartnershipStatus.Pending)
            return Result.Failure("partnerships.invalid_status", "Only pending requests can be approved.");

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

        partnership.Status = OrganisationRecruitmentPartnershipStatus.Active;
        partnership.Scope = scope;
        partnership.RecruiterCanCreateUnclaimedCompanyJobs = dto.RecruiterCanCreateUnclaimedCompanyJobs;
        partnership.RecruiterCanPublishJobs = dto.RecruiterCanPublishJobs;
        partnership.RecruiterCanManageCandidates = dto.RecruiterCanManageCandidates;
        partnership.ApprovedByUserId = currentUserId;
        partnership.ApprovedUtc = utcNow;
        partnership.UpdatedUtc = utcNow;
        partnership.UpdatedByUserId = currentUserId;

        if (!string.IsNullOrWhiteSpace(dto.Notes))
            partnership.Notes = dto.Notes;

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
