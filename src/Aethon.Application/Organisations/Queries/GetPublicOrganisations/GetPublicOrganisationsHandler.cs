using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Organisations.Queries.GetPublicOrganisations;

public sealed class GetPublicOrganisationsHandler
{
    private readonly AethonDbContext _db;

    public GetPublicOrganisationsHandler(AethonDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PublicOrganisationsPageDto>> HandleAsync(
        string? search,
        bool verifiedOnly,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Set<Data.Entities.Organisation>()
            .AsNoTracking()
            .Where(o => o.IsPublicProfileEnabled && o.Status == OrganisationStatus.Active);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(o => o.Name.ToLower().Contains(s));
        }

        if (verifiedOnly)
            query = query.Where(o => o.VerificationTier != VerificationTier.None);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(o => o.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new PublicOrganisationListItemDto
            {
                OrganisationId = o.Id,
                Name = o.Name,
                Slug = o.Slug,
                LogoUrl = o.LogoUrl,
                Summary = o.Summary,
                PublicLocationText = o.PublicLocationText,
                IsVerified = o.VerificationTier != VerificationTier.None,
                VerificationTier = o.VerificationTier,
                Industry = o.Industry,
                CompanySize = o.CompanySize,
                ActiveJobCount = o.OwnedJobs.Count(j => j.Status == JobStatus.Published)
            })
            .ToListAsync(ct);

        return Result<PublicOrganisationsPageDto>.Success(new PublicOrganisationsPageDto
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }
}
