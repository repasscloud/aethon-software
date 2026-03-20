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

    public async Task<Result<List<PublicJobListItemDto>>> HandleAsync(CancellationToken ct = default)
    {
        var jobs = await _db.Jobs
            .AsNoTracking()
            .Where(j => j.Status == JobStatus.Published && j.Visibility == JobVisibility.Public)
            .OrderByDescending(j => j.PublishedUtc)
            .Take(100)
            .Select(j => new PublicJobListItemDto
            {
                Id = j.Id,
                Title = j.Title,
                OrganisationName = j.OwnedByOrganisation.Name,
                OrganisationSlug = j.OwnedByOrganisation.Slug,
                OrganisationLogoUrl = j.OwnedByOrganisation.LogoUrl,
                Department = j.Department,
                LocationText = j.LocationText,
                WorkplaceType = j.WorkplaceType,
                EmploymentType = j.EmploymentType,
                SalaryFrom = j.SalaryFrom,
                SalaryTo = j.SalaryTo,
                SalaryCurrency = j.SalaryCurrency,
                PublishedUtc = j.PublishedUtc
            })
            .ToListAsync(ct);

        return Result<List<PublicJobListItemDto>>.Success(jobs);
    }
}
