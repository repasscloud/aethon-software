using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.RecruiterCompanies.Commands.CancelRecruiterCompanyRequest;

public sealed class CancelRecruiterCompanyRequestHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CancelRecruiterCompanyRequestHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> HandleAsync(Guid partnershipId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure("auth.unauthenticated", "Not authenticated.");

        var myMembership = await _db.OrganisationMemberships
            .AsNoTracking()
            .Where(m => m.UserId == _currentUser.UserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (myMembership is null)
            return Result.Failure("organisations.not_found", "No active recruiter membership found.");

        var recruiterOrgId = myMembership.OrganisationId;

        var partnership = await _db.OrganisationRecruitmentPartnerships
            .FirstOrDefaultAsync(p => p.Id == partnershipId && p.RecruiterOrganisationId == recruiterOrgId, ct);

        if (partnership is null)
            return Result.Failure("partnerships.not_found", "The partnership request was not found.");

        if (partnership.Status != OrganisationRecruitmentPartnershipStatus.Pending)
            return Result.Failure("partnerships.invalid_status", "Cannot cancel a non-pending request.");

        partnership.Status = OrganisationRecruitmentPartnershipStatus.Revoked;
        partnership.UpdatedUtc = _dateTimeProvider.UtcNow;
        partnership.UpdatedByUserId = _currentUser.UserId;

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
