using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Queries.GetOrganisationMemberDetail;

public sealed class GetOrganisationMemberDetailHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public GetOrganisationMemberDetailHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<OrganisationMemberDetailDto>> HandleAsync(Guid targetUserId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<OrganisationMemberDetailDto>.Failure("auth.unauthenticated", "Not authenticated.");

        // Caller must be active Owner or Admin in the org
        var myMembership = await _db.OrganisationMemberships
            .AsNoTracking()
            .Where(m => m.UserId == _currentUser.UserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (myMembership is null)
            return Result<OrganisationMemberDetailDto>.Failure("organisations.not_found", "No active organisation membership found.");

        var callerIsOwnerOrAdmin = myMembership.IsOwner ||
            myMembership.CompanyRole is CompanyRole.Owner or CompanyRole.Admin ||
            myMembership.RecruiterRole is RecruiterRole.Owner or RecruiterRole.Admin;

        if (!callerIsOwnerOrAdmin)
            return Result<OrganisationMemberDetailDto>.Failure("organisations.forbidden", "Insufficient permissions to view member details.");

        var orgId = myMembership.OrganisationId;

        // Load target membership + user
        var membership = await _db.OrganisationMemberships
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => m.OrganisationId == orgId && m.UserId == targetUserId)
            .FirstOrDefaultAsync(ct);

        if (membership is null)
            return Result<OrganisationMemberDetailDto>.Failure("organisations.member.not_found", "Member not found in this organisation.");

        // Load their member profile if it exists
        var profile = await _db.OrganisationMemberProfiles
            .AsNoTracking()
            .Where(p => p.OrganisationId == orgId && p.UserId == targetUserId)
            .FirstOrDefaultAsync(ct);

        return Result<OrganisationMemberDetailDto>.Success(new OrganisationMemberDetailDto
        {
            UserId = membership.UserId,
            DisplayName = membership.User.DisplayName,
            Email = membership.User.Email ?? "",
            IsOwner = membership.IsOwner,
            CompanyRole = membership.CompanyRole?.ToString(),
            RecruiterRole = membership.RecruiterRole?.ToString(),
            MembershipStatus = membership.Status.ToString(),
            JoinedUtc = membership.JoinedUtc,
            IsIdentityVerified = membership.User.IsIdentityVerified,
            EmailConfirmed = membership.User.EmailConfirmed,
            Profile = profile is null ? null : new OrganisationMemberProfileDto
            {
                Slug = profile.Slug,
                JobTitle = profile.JobTitle,
                Bio = profile.Bio,
                ProfilePictureUrl = profile.ProfilePictureUrl,
                PublicEmail = profile.PublicEmail,
                PublicPhone = profile.PublicPhone,
                LinkedInUrl = profile.LinkedInUrl,
                IsPublicProfileEnabled = profile.IsPublicProfileEnabled
            }
        });
    }
}
