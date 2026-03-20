using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Aethon.Shared.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Jobs.Queries.GetPublicJobDetail;

public sealed class GetPublicJobDetailHandler
{
    private readonly AethonDbContext _db;

    public GetPublicJobDetailHandler(AethonDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PublicJobDetailDto>> HandleAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _db.Jobs
            .AsNoTracking()
            .Where(j => j.Id == jobId
                     && j.Status == JobStatus.Published
                     && j.Visibility == JobVisibility.Public)
            .Select(j => new PublicJobDetailDto
            {
                Id = j.Id,
                Title = j.Title,
                Department = j.Department,
                LocationText = j.LocationText,
                WorkplaceType = j.WorkplaceType,
                EmploymentType = j.EmploymentType,
                Description = j.Description,
                Requirements = j.Requirements,
                Benefits = j.Benefits,
                SalaryFrom = j.SalaryFrom,
                SalaryTo = j.SalaryTo,
                SalaryCurrency = j.SalaryCurrency,
                PublishedUtc = j.PublishedUtc,
                Organisation = new PublicOrganisationProfileDto
                {
                    OrganisationId = j.OwnedByOrganisation.Id,
                    OrganisationType = j.OwnedByOrganisation.Type.ToString().ToLowerInvariant(),
                    Name = j.OwnedByOrganisation.Name,
                    Slug = j.OwnedByOrganisation.Slug,
                    LogoUrl = j.OwnedByOrganisation.LogoUrl,
                    WebsiteUrl = j.OwnedByOrganisation.WebsiteUrl,
                    Summary = j.OwnedByOrganisation.Summary,
                    PublicLocationText = j.OwnedByOrganisation.PublicLocationText,
                    PublicContactEmail = j.OwnedByOrganisation.PublicContactEmail,
                    PublicContactPhone = j.OwnedByOrganisation.PublicContactPhone
                }
            })
            .FirstOrDefaultAsync(ct);

        if (job is null)
            return Result<PublicJobDetailDto>.Failure("jobs.not_found", "Job not found.");

        return Result<PublicJobDetailDto>.Success(job);
    }
}
