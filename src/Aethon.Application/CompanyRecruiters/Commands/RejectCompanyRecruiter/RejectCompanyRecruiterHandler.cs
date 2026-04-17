using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.CompanyRecruiters;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.CompanyRecruiters.Commands.RejectCompanyRecruiter;

public sealed class RejectCompanyRecruiterHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RejectCompanyRecruiterHandler(
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
        RejectRecruiterCompanyRequestDto dto,
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

        var companyOrgId = myMembership.OrganisationId;

        var partnership = await _db.OrganisationRecruitmentPartnerships
            .FirstOrDefaultAsync(p => p.Id == partnershipId && p.CompanyOrganisationId == companyOrgId, ct);

        if (partnership is null)
            return Result.Failure("partnerships.not_found", "The partnership request was not found.");

        if (partnership.Status != OrganisationRecruitmentPartnershipStatus.Pending)
            return Result.Failure("partnerships.invalid_status", "Only pending requests can be rejected.");

        var utcNow = _dateTimeProvider.UtcNow;

        partnership.Status = OrganisationRecruitmentPartnershipStatus.Rejected;
        partnership.Notes = dto.Reason;
        partnership.UpdatedUtc = utcNow;
        partnership.UpdatedByUserId = currentUserId;

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
