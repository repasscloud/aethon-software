using System.Text.Json;
using System.Text.Json.Serialization;
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
        var utcNow = DateTime.UtcNow;

        var raw = await _db.Jobs
            .AsNoTracking()
            .Where(j => j.Id == jobId
                     && j.Status == JobStatus.Published
                     && j.Visibility == JobVisibility.Public
                     && (j.PostingExpiresUtc == null || j.PostingExpiresUtc > utcNow))
            .Select(j => new
            {
                j.Id,
                j.Title,
                j.Department,
                j.LocationText,
                j.WorkplaceType,
                j.EmploymentType,
                j.Description,
                j.Requirements,
                j.Benefits,
                j.SalaryFrom,
                j.SalaryTo,
                j.SalaryCurrency,
                j.PublishedUtc,
                j.Category,
                j.BenefitsTags,
                j.Regions,
                j.ApplicationSpecialRequirements,
                j.ExternalApplicationUrl,
                j.ApplicationEmail,
                j.HasCommission,
                j.OteFrom,
                j.OteTo,
                j.IsImmediateStart,
                j.VideoYouTubeId,
                j.VideoVimeoId,
                j.Summary,
                j.ScreeningQuestionsJson,
                OrgId = j.OwnedByOrganisation.Id,
                OrgType = j.OwnedByOrganisation.Type,
                OrgName = j.OwnedByOrganisation.Name,
                OrgSlug = j.OwnedByOrganisation.Slug,
                OrgLogoUrl = j.OwnedByOrganisation.LogoUrl,
                OrgWebsiteUrl = j.OwnedByOrganisation.WebsiteUrl,
                OrgSummary = j.OwnedByOrganisation.Summary,
                OrgPublicLocationText = j.OwnedByOrganisation.PublicLocationText,
                OrgPublicContactEmail = j.OwnedByOrganisation.PublicContactEmail,
                OrgPublicContactPhone = j.OwnedByOrganisation.PublicContactPhone,
                OrgIsEqualOpportunityEmployer = j.OwnedByOrganisation.IsEqualOpportunityEmployer,
                OrgIsAccessibleWorkplace = j.OwnedByOrganisation.IsAccessibleWorkplace,
                OrgLinkedInUrl = j.OwnedByOrganisation.LinkedInUrl,
                OrgTwitterHandle = j.OwnedByOrganisation.TwitterHandle,
                OrgFacebookUrl = j.OwnedByOrganisation.FacebookUrl,
                OrgTikTokHandle = j.OwnedByOrganisation.TikTokHandle,
                OrgInstagramHandle = j.OwnedByOrganisation.InstagramHandle,
                OrgYouTubeUrl = j.OwnedByOrganisation.YouTubeUrl
            })
            .FirstOrDefaultAsync(ct);

        if (raw is null)
            return Result<PublicJobDetailDto>.Failure("jobs.not_found", "Job not found.");

        var job = new PublicJobDetailDto
        {
            Id = raw.Id,
            Title = raw.Title,
            Department = raw.Department,
            LocationText = raw.LocationText,
            WorkplaceType = raw.WorkplaceType,
            EmploymentType = raw.EmploymentType,
            Description = raw.Description,
            Requirements = raw.Requirements,
            Benefits = raw.Benefits,
            SalaryFrom = raw.SalaryFrom,
            SalaryTo = raw.SalaryTo,
            SalaryCurrency = raw.SalaryCurrency,
            PublishedUtc = raw.PublishedUtc,
            Category = raw.Category,
            Summary = raw.Summary,
            ApplicationSpecialRequirements = raw.ApplicationSpecialRequirements,
            ExternalApplicationUrl = raw.ExternalApplicationUrl,
            ApplicationEmail = raw.ApplicationEmail,
            HasCommission = raw.HasCommission,
            OteFrom = raw.OteFrom,
            OteTo = raw.OteTo,
            IsImmediateStart = raw.IsImmediateStart,
            VideoYouTubeId = raw.VideoYouTubeId,
            VideoVimeoId = raw.VideoVimeoId,
            ScreeningQuestionsJson = raw.ScreeningQuestionsJson,
            BenefitsTags = raw.BenefitsTags is not null
                ? JsonSerializer.Deserialize<List<string>>(raw.BenefitsTags) ?? []
                : [],
            Regions = raw.Regions is not null
                ? JsonSerializer.Deserialize<List<JobRegion>>(raw.Regions, _enumJson) ?? []
                : [],
            Organisation = new PublicOrganisationProfileDto
            {
                OrganisationId = raw.OrgId,
                OrganisationType = raw.OrgType.ToString().ToLowerInvariant(),
                Name = raw.OrgName,
                Slug = raw.OrgSlug,
                LogoUrl = raw.OrgLogoUrl,
                WebsiteUrl = raw.OrgWebsiteUrl,
                Summary = raw.OrgSummary,
                PublicLocationText = raw.OrgPublicLocationText,
                PublicContactEmail = raw.OrgPublicContactEmail,
                PublicContactPhone = raw.OrgPublicContactPhone,
                IsEqualOpportunityEmployer = raw.OrgIsEqualOpportunityEmployer,
                IsAccessibleWorkplace = raw.OrgIsAccessibleWorkplace,
                LinkedInUrl = raw.OrgLinkedInUrl,
                TwitterHandle = raw.OrgTwitterHandle,
                FacebookUrl = raw.OrgFacebookUrl,
                TikTokHandle = raw.OrgTikTokHandle,
                InstagramHandle = raw.OrgInstagramHandle,
                YouTubeUrl = raw.OrgYouTubeUrl
            }
        };

        return Result<PublicJobDetailDto>.Success(job);
    }

    private static readonly JsonSerializerOptions _enumJson = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
