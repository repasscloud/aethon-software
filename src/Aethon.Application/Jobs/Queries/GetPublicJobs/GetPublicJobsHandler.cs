using System.Text.Json;
using System.Text.Json.Serialization;
using Aethon.Application.Common.Results;
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

        // Region filter (text search in JSON array — stored as string names e.g. ["Oceania"])
        if (q.Region.HasValue)
        {
            var regionStr = $"\"{q.Region.Value.ToString()}\"";
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

        // Salary range filter
        if (q.SalaryMin.HasValue)
            dbQuery = dbQuery.Where(j => j.SalaryTo >= q.SalaryMin.Value || j.OteTo >= q.SalaryMin.Value);

        if (q.SalaryMax.HasValue)
            dbQuery = dbQuery.Where(j => j.SalaryFrom <= q.SalaryMax.Value);

        // Verified employers only
        if (q.VerifiedOnly)
            dbQuery = dbQuery.Where(j => j.OwnedByOrganisation.VerificationTier != VerificationTier.None);

        // Workplace type filter
        if (q.WorkplaceType.HasValue)
            dbQuery = dbQuery.Where(j => j.WorkplaceType == q.WorkplaceType.Value);

        // Immediate start filter
        if (q.ImmediateStart)
            dbQuery = dbQuery.Where(j => j.IsImmediateStart);

        var raw = await dbQuery
            .OrderByDescending(j => j.StickyUntilUtc > DateTime.UtcNow)
            .ThenByDescending(j => j.IsHighlighted)
            .ThenByDescending(j => j.PublishedUtc)
            .Take(200)
            .Select(j => new
            {
                j.Id,
                j.Title,
                j.Summary,
                OrganisationName = j.OwnedByOrganisation.Name,
                OrganisationSlug = j.OwnedByOrganisation.Slug,
                OrganisationLogoUrl = j.OwnedByOrganisation.LogoUrl,
                OrganisationIsVerified = j.OwnedByOrganisation.VerificationTier != VerificationTier.None,
                j.Department,
                j.LocationText,
                j.WorkplaceType,
                j.EmploymentType,
                j.SalaryFrom,
                j.SalaryTo,
                j.SalaryCurrency,
                j.HasCommission,
                j.OteFrom,
                j.OteTo,
                j.PublishedUtc,
                j.Category,
                j.Regions,
                j.BenefitsTags,
                j.IsHighlighted,
                j.IsImmediateStart,
                j.IncludeCompanyLogo
            })
            .ToListAsync(ct);

        var enumJson = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };

        var jobs = raw.Select(j => new PublicJobListItemDto
        {
            Id = j.Id,
            Title = j.Title,
            Summary = j.Summary,
            OrganisationName = j.OrganisationName,
            OrganisationSlug = j.OrganisationSlug,
            OrganisationLogoUrl = j.IncludeCompanyLogo ? j.OrganisationLogoUrl : null,
            OrganisationIsVerified = j.OrganisationIsVerified,
            Department = j.Department,
            LocationText = j.LocationText,
            WorkplaceType = j.WorkplaceType,
            EmploymentType = j.EmploymentType,
            SalaryFrom = j.SalaryFrom,
            SalaryTo = j.SalaryTo,
            SalaryCurrency = j.SalaryCurrency,
            HasCommission = j.HasCommission,
            OteFrom = j.OteFrom,
            OteTo = j.OteTo,
            PublishedUtc = j.PublishedUtc,
            Category = j.Category,
            Regions = j.Regions is not null
                ? JsonSerializer.Deserialize<List<JobRegion>>(j.Regions, enumJson) ?? []
                : [],
            BenefitsTags = j.BenefitsTags is not null
                ? JsonSerializer.Deserialize<List<string>>(j.BenefitsTags) ?? []
                : [],
            IsHighlighted = j.IsHighlighted,
            IsImmediateStart = j.IsImmediateStart,
            IncludeCompanyLogo = j.IncludeCompanyLogo
        }).ToList();

        return Result<List<PublicJobListItemDto>>.Success(jobs);
    }
}
