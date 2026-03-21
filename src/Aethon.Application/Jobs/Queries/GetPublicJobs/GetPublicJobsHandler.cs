using System.Text.Json;
using Aethon.Application.Common.Results;
using Aethon.Shared.Enums;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Jobs.Queries.GetPublicJobs;

public sealed class GetPublicJobsHandler
{
    private readonly AethonDbContext _db;

    public GetPublicJobsHandler(AethonDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<PublicJobListItemDto>>> HandleAsync(
        GetPublicJobsQuery? query = null,
        CancellationToken ct = default)
    {
        var q = query ?? new GetPublicJobsQuery();

        var utcNow = DateTime.UtcNow;

        var dbQuery = _db.Jobs
            .AsNoTracking()
            .Where(j => j.Status == JobStatus.Published
                     && j.Visibility == JobVisibility.Public
                     && (j.PostingExpiresUtc == null || j.PostingExpiresUtc > utcNow));

        // City / location filter
        if (!string.IsNullOrWhiteSpace(q.City))
        {
            var city = q.City.ToLower();
            dbQuery = dbQuery.Where(j => j.LocationText != null && j.LocationText.ToLower().Contains(city));
        }

        // Date range filter
        if (q.DateRange.HasValue)
        {
            dbQuery = q.DateRange.Value switch
            {
                DateRangeFilter.Today       => dbQuery.Where(j => j.PublishedUtc >= utcNow.Date),
                DateRangeFilter.Last3Days   => dbQuery.Where(j => j.PublishedUtc >= utcNow.AddDays(-3)),
                DateRangeFilter.LastWeek    => dbQuery.Where(j => j.PublishedUtc >= utcNow.AddDays(-7)),
                DateRangeFilter.Last14Days  => dbQuery.Where(j => j.PublishedUtc >= utcNow.AddDays(-14)),
                DateRangeFilter.Last30Days  => dbQuery.Where(j => j.PublishedUtc >= utcNow.AddDays(-30)),
                DateRangeFilter.Last60Days  => dbQuery.Where(j => j.PublishedUtc >= utcNow.AddDays(-60)),
                DateRangeFilter.Last90Days  => dbQuery.Where(j => j.PublishedUtc >= utcNow.AddDays(-90)),
                DateRangeFilter.Over90Days  => dbQuery.Where(j => j.PublishedUtc < utcNow.AddDays(-90)),
                _                           => dbQuery
            };
        }

        // Category filter
        if (q.Category.HasValue)
            dbQuery = dbQuery.Where(j => j.Category == q.Category.Value);

        // Region filter (text search in JSON array)
        if (q.Region.HasValue)
        {
            var regionStr = q.Region.Value.ToString();
            dbQuery = dbQuery.Where(j => j.Regions != null && j.Regions.Contains(regionStr));
        }

        // Country filter (stored as JSON string, do a text search)
        if (!string.IsNullOrWhiteSpace(q.Country))
        {
            var country = q.Country.ToLower();
            dbQuery = dbQuery.Where(j => j.Countries != null && j.Countries.ToLower().Contains(country));
        }

        // Organisation filter
        if (!string.IsNullOrWhiteSpace(q.OrganisationSlug))
        {
            var slug = q.OrganisationSlug.ToLower();
            dbQuery = dbQuery.Where(j => j.OwnedByOrganisation.Slug.ToLower() == slug);
        }

        // Keywords filter
        if (!string.IsNullOrWhiteSpace(q.Keywords))
        {
            var kw = q.Keywords.ToLower();
            dbQuery = dbQuery.Where(j =>
                (j.Keywords != null && j.Keywords.ToLower().Contains(kw)) ||
                j.Title.ToLower().Contains(kw) ||
                j.Description.ToLower().Contains(kw));
        }

        var raw = await dbQuery
            .OrderByDescending(j => j.StickyUntilUtc > DateTime.UtcNow)
            .ThenByDescending(j => j.IsHighlighted)
            .ThenByDescending(j => j.PublishedUtc)
            .Take(200)
            .Select(j => new
            {
                j.Id,
                j.Title,
                OrganisationName = j.OwnedByOrganisation.Name,
                OrganisationSlug = j.OwnedByOrganisation.Slug,
                OrganisationLogoUrl = j.OwnedByOrganisation.LogoUrl,
                j.Department,
                j.LocationText,
                j.WorkplaceType,
                j.EmploymentType,
                j.SalaryFrom,
                j.SalaryTo,
                j.SalaryCurrency,
                j.PublishedUtc,
                j.Category,
                j.Regions,
                j.BenefitsTags,
                j.IsHighlighted,
                j.IncludeCompanyLogo
            })
            .ToListAsync(ct);

        var jobs = raw.Select(j => new PublicJobListItemDto
        {
            Id = j.Id,
            Title = j.Title,
            OrganisationName = j.OrganisationName,
            OrganisationSlug = j.OrganisationSlug,
            OrganisationLogoUrl = j.IncludeCompanyLogo ? j.OrganisationLogoUrl : null,
            Department = j.Department,
            LocationText = j.LocationText,
            WorkplaceType = j.WorkplaceType,
            EmploymentType = j.EmploymentType,
            SalaryFrom = j.SalaryFrom,
            SalaryTo = j.SalaryTo,
            SalaryCurrency = j.SalaryCurrency,
            PublishedUtc = j.PublishedUtc,
            Category = j.Category,
            Regions = j.Regions is not null
                ? JsonSerializer.Deserialize<List<JobRegion>>(j.Regions) ?? []
                : [],
            BenefitsTags = j.BenefitsTags is not null
                ? JsonSerializer.Deserialize<List<string>>(j.BenefitsTags) ?? []
                : [],
            IsHighlighted = j.IsHighlighted,
            IncludeCompanyLogo = j.IncludeCompanyLogo
        }).ToList();

        return Result<List<PublicJobListItemDto>>.Success(jobs);
    }
}
