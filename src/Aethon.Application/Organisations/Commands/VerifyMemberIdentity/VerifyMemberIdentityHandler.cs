using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Commands.VerifyMemberIdentity;

public sealed class VerifyMemberIdentityHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public VerifyMemberIdentityHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> HandleAsync(Guid targetUserId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure("auth.unauthenticated", "Not authenticated.");

        // Only the org owner can manually verify members
        var myMembership = await _db.OrganisationMemberships
            .Where(m => m.UserId == _currentUser.UserId && m.Status == MembershipStatus.Active && m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (myMembership is null)
            return Result.Failure("organisations.forbidden", "Only the organisation owner can manually verify members.");

        var orgId = myMembership.OrganisationId;

        // Confirm the target is an active member of the same org
        var targetMembership = await _db.OrganisationMemberships
            .Where(m => m.OrganisationId == orgId && m.UserId == targetUserId && m.Status == MembershipStatus.Active)
            .FirstOrDefaultAsync(ct);

        if (targetMembership is null)
            return Result.Failure("organisations.member.not_found", "Active member not found in this organisation.");

        // Load the user record
        var user = await _db.Users
            .Where(u => u.Id == targetUserId)
            .FirstOrDefaultAsync(ct);

        if (user is null)
            return Result.Failure("organisations.member.not_found", "User account not found.");

        if (user.IsIdentityVerified)
            return Result.Failure("organisations.member.already_verified", "This member's identity is already verified.");

        // Mark verified on the user account
        user.IsIdentityVerified = true;
        user.IdentityVerifiedUtc = DateTime.UtcNow;
        user.IdentityVerificationNotes = $"Manually verified by organisation owner (UserId: {_currentUser.UserId})";

        // Create an audit record in IdentityVerificationRequests
        var auditRecord = new IdentityVerificationRequest
        {
            Id = Guid.NewGuid(),
            UserId = targetUserId,
            FullName = user.DisplayName,
            EmailAddress = user.Email ?? "",
            PhoneNumber = "",
            AdditionalNotes = "Manually verified by organisation owner.",
            Status = VerificationRequestStatus.Approved,
            ReviewedByUserId = _currentUser.UserId,
            ReviewedUtc = DateTime.UtcNow,
            ReviewerType = VerificationReviewerType.OrgOwner,
            ReviewNotes = $"Verified by org owner (UserId: {_currentUser.UserId})",
            CreatedUtc = DateTime.UtcNow,
            CreatedByUserId = _currentUser.UserId
        };

        _db.IdentityVerificationRequests.Add(auditRecord);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
