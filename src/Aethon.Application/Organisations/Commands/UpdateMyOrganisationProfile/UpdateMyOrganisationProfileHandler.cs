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
        org.PrimaryContactName = request.PrimaryContactName?.Trim();
        org.PrimaryContactEmail = request.PrimaryContactEmail?.Trim();
        org.PrimaryContactPhoneDialCode = request.PrimaryContactPhoneDialCode?.Trim();
        org.PrimaryContactPhone = request.PrimaryContactPhone?.Trim();
        org.PublicContactEmail = request.PublicContactEmail?.Trim();
        org.PublicContactPhoneDialCode = request.PublicContactPhoneDialCode?.Trim();
        org.PublicContactPhone = request.PublicContactPhone?.Trim();
        org.RegisteredAddressLine1 = request.RegisteredAddressLine1?.Trim();
        org.RegisteredAddressLine2 = request.RegisteredAddressLine2?.Trim();
        org.RegisteredCity = request.RegisteredCity?.Trim();
        org.RegisteredState = request.RegisteredState?.Trim();
        org.RegisteredPostcode = request.RegisteredPostcode?.Trim();
        org.RegisteredCountry = request.RegisteredCountry?.Trim();
        org.RegisteredCountryCode = request.RegisteredCountryCode?.Trim();
        org.TaxRegistrationNumber = request.TaxRegistrationNumber?.Trim();
        org.BusinessRegistrationNumber = request.BusinessRegistrationNumber?.Trim();
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
