using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Queries.GetMyOrganisationProfile;

public sealed class GetMyOrganisationProfileHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;

    public GetMyOrganisationProfileHandler(AethonDbContext db, ICurrentUserAccessor currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<OrganisationProfileDto>> HandleAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<OrganisationProfileDto>.Failure("auth.unauthenticated", "Not authenticated.");

        var membership = await _db.OrganisationMemberships
            .AsNoTracking()
            .Include(m => m.Organisation)
            .Where(m => m.UserId == _currentUser.UserId && m.Status == Shared.Enums.MembershipStatus.Active)
            .OrderByDescending(m => m.IsOwner)
            .FirstOrDefaultAsync(ct);

        if (membership is null)
            return Result<OrganisationProfileDto>.Failure("organisations.not_found", "No active organisation membership found.");

        var org = membership.Organisation;

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
            PrimaryContactName = org.PrimaryContactName,
            PrimaryContactEmail = org.PrimaryContactEmail,
            PrimaryContactPhoneDialCode = org.PrimaryContactPhoneDialCode,
            PrimaryContactPhone = org.PrimaryContactPhone,
            PublicContactEmail = org.PublicContactEmail,
            PublicContactPhoneDialCode = org.PublicContactPhoneDialCode,
            PublicContactPhone = org.PublicContactPhone,
            RegisteredAddressLine1 = org.RegisteredAddressLine1,
            RegisteredAddressLine2 = org.RegisteredAddressLine2,
            RegisteredCity = org.RegisteredCity,
            RegisteredState = org.RegisteredState,
            RegisteredPostcode = org.RegisteredPostcode,
            RegisteredCountry = org.RegisteredCountry,
            RegisteredCountryCode = org.RegisteredCountryCode,
            TaxRegistrationNumber = org.TaxRegistrationNumber,
            BusinessRegistrationNumber = org.BusinessRegistrationNumber,
            IsPublicProfileEnabled = org.IsPublicProfileEnabled,
            IsEqualOpportunityEmployer = org.IsEqualOpportunityEmployer,
            IsAccessibleWorkplace = org.IsAccessibleWorkplace,
            IsVerified = org.IsVerified,
            VerificationTier = org.VerificationTier,
            VerificationPendingTier = org.VerificationPendingTier,
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
