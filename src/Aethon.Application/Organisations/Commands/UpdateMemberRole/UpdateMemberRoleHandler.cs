using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Commands.UpdateMemberRole;

public sealed class UpdateMemberRoleHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public UpdateMemberRoleHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> HandleAsync(Guid targetUserId, UpdateMemberRoleRequestDto request, CancellationToken ct = default)
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
            return Result.Failure("organisations.forbidden", "Insufficient permissions to update member roles.");

        var orgId = myMembership.OrganisationId;

        var target = await _db.OrganisationMemberships
            .Where(m => m.OrganisationId == orgId && m.UserId == targetUserId)
            .FirstOrDefaultAsync(ct);

        if (target is null)
            return Result.Failure("organisations.member.not_found", "Member not found in this organisation.");

        // Non-owner admins cannot change the role of an owner
        if (target.IsOwner && !myMembership.IsOwner)
            return Result.Failure("organisations.forbidden", "Only the organisation owner can change another owner's role.");

        // Cannot change your own role
        if (targetUserId == _currentUser.UserId)
            return Result.Failure("organisations.member.invalid", "You cannot change your own role.");

        if (!string.IsNullOrWhiteSpace(request.CompanyRole))
        {
            if (!Enum.TryParse<CompanyRole>(request.CompanyRole, ignoreCase: true, out var parsedRole))
                return Result.Failure("organisations.member.invalid_role", $"Invalid company role: {request.CompanyRole}.");
            target.CompanyRole = parsedRole;
        }

        if (!string.IsNullOrWhiteSpace(request.RecruiterRole))
        {
            if (!Enum.TryParse<RecruiterRole>(request.RecruiterRole, ignoreCase: true, out var parsedRole))
                return Result.Failure("organisations.member.invalid_role", $"Invalid recruiter role: {request.RecruiterRole}.");
            target.RecruiterRole = parsedRole;
        }

        target.UpdatedByUserId = _currentUser.UserId;

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
