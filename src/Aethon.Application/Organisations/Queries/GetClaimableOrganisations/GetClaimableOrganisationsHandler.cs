using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Queries.GetClaimableOrganisations;

public sealed class GetClaimableOrganisationsHandler
{
    private readonly AethonDbContext _db;

    public GetClaimableOrganisationsHandler(AethonDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<ClaimableOrganisationDto>>> HandleAsync(
        string? searchTerm = null,
        CancellationToken ct = default)
    {
        var query = _db.Organisations
            .Include(o => o.PrimaryDomain)
            .Where(o => o.ClaimStatus == OrganisationClaimStatus.Unclaimed
                     && o.Status == OrganisationStatus.Claimable);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(o =>
                o.Name.Contains(searchTerm) ||
                (o.Slug != null && o.Slug.Contains(searchTerm)));
        }

        var orgs = await query
            .Take(50)
            .ToListAsync(ct);

        var results = new List<ClaimableOrganisationDto>(orgs.Count);

        foreach (var org in orgs)
        {
            var hasActiveClaim = await _db.OrganisationClaimRequests
                .AnyAsync(r => r.OrganisationId == org.Id && r.Status == ClaimRequestStatus.Pending, ct);

            results.Add(new ClaimableOrganisationDto
            {
                Id = org.Id,
                Name = org.Name,
                Slug = org.Slug ?? string.Empty,
                Domain = org.PrimaryDomain?.Domain,
                HasActiveClaim = hasActiveClaim
            });
        }

        return Result<List<ClaimableOrganisationDto>>.Success(results);
    }
}
