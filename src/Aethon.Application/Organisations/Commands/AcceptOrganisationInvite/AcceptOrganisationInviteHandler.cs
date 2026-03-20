using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Commands.AcceptOrganisationInvite;

public sealed class AcceptOrganisationInviteHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public AcceptOrganisationInviteHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> HandleAsync(
        AcceptOrganisationInviteRequestDto request,
        CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure("auth.unauthenticated", "Not authenticated.");

        var invitation = await _db.OrganisationInvitations
            .FirstOrDefaultAsync(i => i.Token == request.Token, ct);

        if (invitation is null)
            return Result.Failure("organisations.invite_not_found", "Invitation not found.");

        if (invitation.Status != InvitationStatus.Pending)
            return Result.Failure("organisations.invite_invalid", "Invitation is no longer valid.");

        if (invitation.ExpiresUtc < DateTime.UtcNow)
        {
            invitation.Status = InvitationStatus.Expired;
            await _db.SaveChangesAsync(ct);
            return Result.Failure("organisations.invite_expired", "Invitation has expired.");
        }

        // Check if user is already a member
        var existing = await _db.OrganisationMemberships
            .FirstOrDefaultAsync(m => m.UserId == _currentUser.UserId && m.OrganisationId == invitation.OrganisationId, ct);

        if (existing is not null && existing.Status == MembershipStatus.Active)
            return Result.Failure("organisations.already_member", "You are already a member of this organisation.");

        var now = DateTime.UtcNow;

        if (existing is not null)
        {
            // Reactivate
            existing.Status = MembershipStatus.Active;
            existing.CompanyRole = invitation.CompanyRole;
            existing.RecruiterRole = invitation.RecruiterRole;
            existing.JoinedUtc = now;
            existing.LeftUtc = null;
        }
        else
        {
            _db.OrganisationMemberships.Add(new OrganisationMembership
            {
                Id = Guid.NewGuid(),
                OrganisationId = invitation.OrganisationId,
                UserId = _currentUser.UserId,
                Status = MembershipStatus.Active,
                IsOwner = invitation.AllowClaimAsOwner,
                CompanyRole = invitation.CompanyRole,
                RecruiterRole = invitation.RecruiterRole,
                JoinedUtc = now,
                CreatedByUserId = _currentUser.UserId,
                CreatedUtc = now
            });
        }

        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedByUserId = _currentUser.UserId;
        invitation.AcceptedUtc = now;

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
