using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Queries.GetPublicOrganisationTeamMember;

public sealed class GetPublicOrganisationTeamMemberHandler
{
    private readonly AethonDbContext _db;

    public GetPublicOrganisationTeamMemberHandler(AethonDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PublicTeamMemberDetailDto>> HandleAsync(
        string orgSlug,
        string memberSlug,
        CancellationToken ct = default)
    {
        var org = await _db.Set<Organisation>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                o => o.Slug != null && o.Slug.ToLower() == orgSlug.ToLower()
                     && o.IsPublicProfileEnabled
                     && o.Status == OrganisationStatus.Active,
                ct);

        if (org is null)
            return Result<PublicTeamMemberDetailDto>.Failure(
                "organisations.not_found",
                "Organisation not found.");

        var profile = await _db.Set<OrganisationMemberProfile>()
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(
                p => p.OrganisationId == org.Id
                     && p.Slug != null && p.Slug.ToLower() == memberSlug.ToLower()
                     && p.IsPublicProfileEnabled,
                ct);

        if (profile is null)
            return Result<PublicTeamMemberDetailDto>.Failure(
                "organisations.member_not_found",
                "Team member not found.");

        return Result<PublicTeamMemberDetailDto>.Success(new PublicTeamMemberDetailDto
        {
            Slug = profile.Slug!,
            DisplayName = profile.User!.DisplayName,
            JobTitle = profile.JobTitle,
            Bio = profile.Bio,
            ProfilePictureUrl = profile.ProfilePictureUrl,
            PublicEmail = profile.PublicEmail,
            PublicPhone = profile.PublicPhone,
            LinkedInUrl = profile.LinkedInUrl,
            IsIdentityVerified = profile.User!.IsIdentityVerified,
            OrgName = org.Name,
            OrgSlug = org.Slug!
        });
    }
}
