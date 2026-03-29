using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Queries.GetPublicOrganisationTeam;

public sealed class GetPublicOrganisationTeamHandler
{
    private readonly AethonDbContext _db;

    public GetPublicOrganisationTeamHandler(AethonDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<PublicTeamMemberDto>>> HandleAsync(
        string orgSlug,
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
            return Result<List<PublicTeamMemberDto>>.Failure(
                "organisations.not_found",
                "Organisation not found.");

        var members = await _db.Set<OrganisationMemberProfile>()
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.OrganisationId == org.Id
                        && p.IsPublicProfileEnabled
                        && p.Slug != null)
            .Select(p => new PublicTeamMemberDto
            {
                Slug = p.Slug!,
                DisplayName = p.User!.DisplayName,
                JobTitle = p.JobTitle,
                ProfilePictureUrl = p.ProfilePictureUrl,
                IsIdentityVerified = p.User!.IsIdentityVerified
            })
            .OrderBy(p => p.DisplayName)
            .ToListAsync(ct);

        return Result<List<PublicTeamMemberDto>>.Success(members);
    }
}
