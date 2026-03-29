using System.Text.RegularExpressions;
using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Commands.UpsertMyMemberProfile;

public sealed class UpsertMyMemberProfileHandler
{
    private static readonly Regex SlugPattern = new(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public UpsertMyMemberProfileHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<OrganisationMemberProfileDto>> HandleAsync(
        UpsertMyMemberProfileRequestDto request,
        CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<OrganisationMemberProfileDto>.Failure("auth.unauthenticated", "Not authenticated.");

        var membership = await _db.OrganisationMemberships
            .Include(m => m.Organisation)
            .Where(m => m.UserId == _currentUser.UserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (membership is null)
            return Result<OrganisationMemberProfileDto>.Failure("organisations.not_found", "No active organisation membership found.");

        var orgId = membership.OrganisationId;

        // Validate and normalise slug
        string? slug = null;
        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            slug = request.Slug.Trim().ToLowerInvariant();

            if (slug.Length < 3 || slug.Length > 60)
                return Result<OrganisationMemberProfileDto>.Failure("member_profile.invalid_slug",
                    "Slug must be between 3 and 60 characters.");

            if (!SlugPattern.IsMatch(slug))
                return Result<OrganisationMemberProfileDto>.Failure("member_profile.invalid_slug",
                    "Slug must contain only lowercase letters, numbers, and hyphens.");

            // Check uniqueness within org (excluding own record)
            var slugTaken = await _db.OrganisationMemberProfiles
                .AnyAsync(p => p.OrganisationId == orgId && p.Slug == slug && p.UserId != _currentUser.UserId, ct);

            if (slugTaken)
                return Result<OrganisationMemberProfileDto>.Failure("member_profile.slug_taken",
                    "This slug is already in use by another team member.");
        }

        // Server-enforce: can only enable public profile if the org has its public profile enabled
        var isPublicProfileEnabled = request.IsPublicProfileEnabled && membership.Organisation.IsPublicProfileEnabled;

        // Upsert the profile
        var profile = await _db.OrganisationMemberProfiles
            .Where(p => p.OrganisationId == orgId && p.UserId == _currentUser.UserId)
            .FirstOrDefaultAsync(ct);

        if (profile is null)
        {
            profile = new OrganisationMemberProfile
            {
                Id = Guid.NewGuid(),
                UserId = _currentUser.UserId,
                OrganisationId = orgId,
                CreatedUtc = DateTime.UtcNow,
                CreatedByUserId = _currentUser.UserId
            };
            _db.OrganisationMemberProfiles.Add(profile);
        }

        profile.Slug = slug;
        profile.JobTitle = request.JobTitle?.Trim();
        profile.Bio = request.Bio?.Trim();
        profile.PublicEmail = request.PublicEmail?.Trim();
        profile.PublicPhone = request.PublicPhone?.Trim();
        profile.LinkedInUrl = request.LinkedInUrl?.Trim();
        profile.IsPublicProfileEnabled = isPublicProfileEnabled;
        profile.ProfilePictureUrl = request.ProfilePictureUrl?.Trim();
        profile.UpdatedUtc = DateTime.UtcNow;
        profile.UpdatedByUserId = _currentUser.UserId;

        await _db.SaveChangesAsync(ct);

        return Result<OrganisationMemberProfileDto>.Success(new OrganisationMemberProfileDto
        {
            Slug = profile.Slug,
            JobTitle = profile.JobTitle,
            Bio = profile.Bio,
            ProfilePictureUrl = profile.ProfilePictureUrl,
            PublicEmail = profile.PublicEmail,
            PublicPhone = profile.PublicPhone,
            LinkedInUrl = profile.LinkedInUrl,
            IsPublicProfileEnabled = profile.IsPublicProfileEnabled
        });
    }
}
