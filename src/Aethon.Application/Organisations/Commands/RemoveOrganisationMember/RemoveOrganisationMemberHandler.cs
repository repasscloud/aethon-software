using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Commands.RemoveOrganisationMember;

public sealed class RemoveOrganisationMemberHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public RemoveOrganisationMemberHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> HandleAsync(Guid targetUserId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure("auth.unauthenticated", "Not authenticated.");

        var myMembership = await _db.OrganisationMemberships
            .Where(m => m.UserId == _currentUser.UserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (myMembership is null)
            return Result.Failure("organisations.not_found", "No active organisation membership found.");

        var callerIsOwnerOrAdmin = myMembership.IsOwner ||
            myMembership.CompanyRole is CompanyRole.Owner or CompanyRole.Admin ||
            myMembership.RecruiterRole is RecruiterRole.Owner or RecruiterRole.Admin;

        if (!callerIsOwnerOrAdmin)
            return Result.Failure("organisations.forbidden", "Insufficient permissions to remove members.");

        if (targetUserId == _currentUser.UserId)
            return Result.Failure("organisations.member.invalid", "You cannot remove yourself from the organisation.");

        var orgId = myMembership.OrganisationId;

        var target = await _db.OrganisationMemberships
            .Where(m => m.OrganisationId == orgId && m.UserId == targetUserId)
            .FirstOrDefaultAsync(ct);

        if (target is null)
            return Result.Failure("organisations.member.not_found", "Member not found in this organisation.");

        if (target.IsOwner)
            return Result.Failure("organisations.forbidden", "The organisation owner cannot be removed.");

        target.Status = MembershipStatus.Revoked;
        target.LeftUtc = DateTime.UtcNow;
        target.UpdatedByUserId = _currentUser.UserId;

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
