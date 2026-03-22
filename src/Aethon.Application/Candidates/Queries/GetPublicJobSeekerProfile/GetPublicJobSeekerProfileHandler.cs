using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Candidates;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Candidates.Queries.GetPublicJobSeekerProfile;

public sealed class GetPublicJobSeekerProfileHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public GetPublicJobSeekerProfileHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Resolves a profile by slug (must be Public) or by userId (access rules apply).
    /// </summary>
    public async Task<Result<PublicJobSeekerProfileDto>> HandleAsync(
        string identifier,
        CancellationToken ct = default)
    {
        var profile = Guid.TryParse(identifier, out var userId)
            ? await _db.JobSeekerProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId, ct)
            : await _db.JobSeekerProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.Slug == identifier.ToLowerInvariant(), ct);

        if (profile is null)
            return Result<PublicJobSeekerProfileDto>.Failure("profile.not_found", "Profile not found.");

        var isStaff = _currentUser.IsStaff;
        var appType  = _currentUser.AppType ?? "";
        var isEmployerOrRecruiter = appType is "employer" or "recruiter";

        var canAccess = profile.ProfileVisibility switch
        {
            ProfileVisibility.Public   => true,
            ProfileVisibility.Unlisted => isStaff || isEmployerOrRecruiter,
            ProfileVisibility.Private  => isStaff,
            _                          => false
        };

        // Slug-based access only works for Public profiles
        if (!Guid.TryParse(identifier, out _) && profile.ProfileVisibility != ProfileVisibility.Public)
            canAccess = false;

        if (!canAccess)
            return Result<PublicJobSeekerProfileDto>.Failure("profile.not_found", "Profile not found.");

        return Result<PublicJobSeekerProfileDto>.Success(new PublicJobSeekerProfileDto
        {
            UserId               = profile.UserId,
            FirstName            = profile.FirstName,
            LastName             = profile.LastName,
            Headline             = profile.Headline,
            Summary              = profile.Summary,
            AboutMe              = profile.AboutMe,
            CurrentLocation      = profile.CurrentLocation,
            LinkedInUrl          = profile.LinkedInUrl,
            OpenToWork           = profile.OpenToWork,
            Slug                 = profile.Slug,
            ProfileVisibility    = profile.ProfileVisibility,
            LastProfileUpdatedUtc = profile.LastProfileUpdatedUtc
        });
    }
}
