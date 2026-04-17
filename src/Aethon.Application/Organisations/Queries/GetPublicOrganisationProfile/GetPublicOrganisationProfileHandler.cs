using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Queries.GetPublicOrganisationProfile;

public sealed class GetPublicOrganisationProfileHandler
{
    private readonly AethonDbContext _db;

    public GetPublicOrganisationProfileHandler(AethonDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PublicOrganisationProfileDto>> HandleAsync(
        string slug,
        CancellationToken ct = default)
    {
        var org = await _db.Set<Data.Entities.Organisation>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                o => o.Slug != null && o.Slug.ToLower() == slug.ToLower()
                     && o.IsPublicProfileEnabled
                     && o.Status == OrganisationStatus.Active,
                ct);

        if (org is null)
            return Result<PublicOrganisationProfileDto>.Failure(
                "organisations.not_found",
                "Organisation not found.");

        var hasPublicTeam = await _db.Set<Data.Entities.OrganisationMemberProfile>()
            .AsNoTracking()
            .AnyAsync(p => p.OrganisationId == org.Id && p.IsPublicProfileEnabled && p.Slug != null, ct);

        return Result<PublicOrganisationProfileDto>.Success(new PublicOrganisationProfileDto
        {
            OrganisationId = org.Id,
            OrganisationType = org.Type.ToString().ToLowerInvariant(),
            Name = org.Name,
            Slug = org.Slug,
            LogoUrl = org.LogoUrl,
            WebsiteUrl = org.WebsiteUrl,
            Summary = org.Summary,
            PublicLocationText = org.PublicLocationText,
            PublicContactEmail = org.PublicContactEmail,
            PublicContactPhone = org.PublicContactPhone,
            IsEqualOpportunityEmployer = org.IsEqualOpportunityEmployer,
            IsAccessibleWorkplace = org.IsAccessibleWorkplace,
            IsVerified = org.IsVerified,
            VerificationTier = org.VerificationTier,
            BannerImageUrl = org.BannerImageUrl,
            CompanySize = org.CompanySize,
            Industry = org.Industry,
            LinkedInUrl = org.LinkedInUrl,
            TwitterHandle = org.TwitterHandle,
            FacebookUrl = org.FacebookUrl,
            TikTokHandle = org.TikTokHandle,
            InstagramHandle = org.InstagramHandle,
            YouTubeUrl = org.YouTubeUrl,
            HasPublicTeam = hasPublicTeam
        });
    }
}
