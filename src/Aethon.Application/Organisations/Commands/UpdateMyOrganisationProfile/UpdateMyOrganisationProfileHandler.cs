using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Commands.UpdateMyOrganisationProfile;

public sealed class UpdateMyOrganisationProfileHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public UpdateMyOrganisationProfileHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<OrganisationProfileDto>> HandleAsync(
        UpdateOrganisationProfileRequestDto request,
        CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<OrganisationProfileDto>.Failure("auth.unauthenticated", "Not authenticated.");

        var membership = await _db.OrganisationMemberships
            .Include(m => m.Organisation)
            .Where(m => m.UserId == _currentUser.UserId && m.Status == MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (membership is null)
            return Result<OrganisationProfileDto>.Failure("organisations.not_found", "No active organisation membership found.");

        if (!membership.IsOwner &&
            membership.CompanyRole is not (CompanyRole.Owner or CompanyRole.Admin) &&
            membership.RecruiterRole is not (RecruiterRole.Owner or RecruiterRole.Admin))
        {
            return Result<OrganisationProfileDto>.Failure("organisations.forbidden", "Insufficient permissions to update organisation profile.");
        }

        var org = membership.Organisation;

        org.Name = request.Name.Trim();
        org.NormalizedName = request.Name.Trim().ToUpperInvariant();
        org.LegalName = request.LegalName?.Trim();
        org.WebsiteUrl = request.WebsiteUrl?.Trim();
        org.Slug = string.IsNullOrWhiteSpace(request.Slug) ? null : request.Slug.Trim().ToLowerInvariant();
        org.LogoUrl = request.LogoUrl?.Trim();
        org.Summary = request.Summary?.Trim();
        org.PublicLocationText = request.PublicLocationText?.Trim();
        org.LocationCity = request.LocationCity?.Trim();
        org.LocationState = request.LocationState?.Trim();
        org.LocationCountry = request.LocationCountry?.Trim();
        org.LocationCountryCode = request.LocationCountryCode?.Trim();
        org.LocationLatitude = request.LocationLatitude;
        org.LocationLongitude = request.LocationLongitude;
        org.LocationPlaceId = request.LocationPlaceId?.Trim();
        org.PublicContactEmail = request.PublicContactEmail?.Trim();
        org.PublicContactPhone = request.PublicContactPhone?.Trim();
        org.IsPublicProfileEnabled = request.IsPublicProfileEnabled;
        org.IsEqualOpportunityEmployer = request.IsEqualOpportunityEmployer;
        org.IsAccessibleWorkplace = request.IsAccessibleWorkplace;
        org.BannerImageUrl = request.BannerImageUrl?.Trim();
        org.CompanySize = request.CompanySize;
        org.Industry = request.Industry;
        org.LinkedInUrl = request.LinkedInUrl?.Trim();
        org.TwitterHandle = request.TwitterHandle?.Trim();
        org.FacebookUrl = request.FacebookUrl?.Trim();
        org.TikTokHandle = request.TikTokHandle?.Trim();
        org.InstagramHandle = request.InstagramHandle?.Trim();
        org.YouTubeUrl = request.YouTubeUrl?.Trim();

        await _db.SaveChangesAsync(ct);

        return Result<OrganisationProfileDto>.Success(new OrganisationProfileDto
        {
            OrganisationId = org.Id,
            OrganisationType = org.Type.ToString().ToLowerInvariant(),
            Name = org.Name,
            LegalName = org.LegalName,
            WebsiteUrl = org.WebsiteUrl,
            Slug = org.Slug,
            LogoUrl = org.LogoUrl,
            Summary = org.Summary,
            PublicLocationText = org.PublicLocationText,
            LocationCity = org.LocationCity,
            LocationState = org.LocationState,
            LocationCountry = org.LocationCountry,
            LocationCountryCode = org.LocationCountryCode,
            LocationLatitude = org.LocationLatitude,
            LocationLongitude = org.LocationLongitude,
            LocationPlaceId = org.LocationPlaceId,
            PublicContactEmail = org.PublicContactEmail,
            PublicContactPhone = org.PublicContactPhone,
            IsPublicProfileEnabled = org.IsPublicProfileEnabled,
            IsEqualOpportunityEmployer = org.IsEqualOpportunityEmployer,
            IsAccessibleWorkplace = org.IsAccessibleWorkplace,
            IsVerified = org.IsVerified,
            BannerImageUrl = org.BannerImageUrl,
            CompanySize = org.CompanySize,
            Industry = org.Industry,
            LinkedInUrl = org.LinkedInUrl,
            TwitterHandle = org.TwitterHandle,
            FacebookUrl = org.FacebookUrl,
            TikTokHandle = org.TikTokHandle,
            InstagramHandle = org.InstagramHandle,
            YouTubeUrl = org.YouTubeUrl
        });
    }
}
