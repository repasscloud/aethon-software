using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Jobs.Queries.GetPublicJobLocations;

public sealed class GetPublicJobLocationsHandler
{
    private readonly AethonDbContext _db;

    public GetPublicJobLocationsHandler(AethonDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<string>>> HandleAsync(string? query, CancellationToken ct = default)
    {
        var utcNow = DateTime.UtcNow;
        var q = query?.Trim() ?? "";

        var dbQuery = _db.Jobs
            .AsNoTracking()
            .Where(j => j.Status == JobStatus.Published
                     && j.Visibility == JobVisibility.Public
                     && j.LocationText != null
                     && j.LocationText != ""
                     && (j.PostingExpiresUtc == null || j.PostingExpiresUtc > utcNow));

        if (!string.IsNullOrWhiteSpace(q))
            dbQuery = dbQuery.Where(j => j.LocationText!.ToLower().Contains(q.ToLower()));

        var locations = await dbQuery
            .Select(j => j.LocationText!)
            .Distinct()
            .OrderBy(l => l)
            .Take(20)
            .ToListAsync(ct);

        return Result<List<string>>.Success(locations);
    }
}
