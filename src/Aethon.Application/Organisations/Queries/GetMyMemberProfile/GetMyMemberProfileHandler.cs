using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Queries.GetMyMemberProfile;

public sealed class GetMyMemberProfileHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public GetMyMemberProfileHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<MyMemberProfileDto>> HandleAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<MyMemberProfileDto>.Failure("auth.unauthenticated", "Not authenticated.");

        var membership = await _db.OrganisationMemberships
            .AsNoTracking()
            .Include(m => m.Organisation)
            .Include(m => m.User)
            .Where(m => m.UserId == _currentUser.UserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (membership is null)
            return Result<MyMemberProfileDto>.Failure("organisations.not_found", "No active organisation membership found.");

        var orgId = membership.OrganisationId;

        var profile = await _db.OrganisationMemberProfiles
            .AsNoTracking()
            .Where(p => p.OrganisationId == orgId && p.UserId == _currentUser.UserId)
            .FirstOrDefaultAsync(ct);

        // Get the most recent verification request status (if any)
        var latestRequest = await _db.IdentityVerificationRequests
            .AsNoTracking()
            .Where(r => r.UserId == _currentUser.UserId)
            .OrderByDescending(r => r.CreatedUtc)
            .Select(r => r.Status)
            .FirstOrDefaultAsync(ct);

        return Result<MyMemberProfileDto>.Success(new MyMemberProfileDto
        {
            UserId = _currentUser.UserId,
            DisplayName = membership.User.DisplayName,
            Email = membership.User.Email ?? "",
            EmailConfirmed = membership.User.EmailConfirmed,
            IsIdentityVerified = membership.User.IsIdentityVerified,
            VerificationRequestStatus = latestRequest == default ? null : latestRequest.ToString(),
            OrganisationId = orgId,
            OrgName = membership.Organisation.Name,
            OrgSlug = membership.Organisation.Slug,
            OrgIsPublicProfileEnabled = membership.Organisation.IsPublicProfileEnabled,
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
