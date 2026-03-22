using System.Text.Json;
using System.Text.Json.Serialization;
using Aethon.Application.Abstractions.Authentication;
using Aethon.Shared.Enums;
using Aethon.Application.Abstractions.Caching;
using Aethon.Application.Common.Caching;
using Aethon.Application.Common.Results;
using Aethon.Application.Organisations.Services;
using Aethon.Data;
using Aethon.Shared.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Jobs.Queries.GetJobById;

public sealed class GetJobByIdHandler
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly OrganisationAccessService _organisationAccessService;
    private readonly IAppCache _cache;

    public GetJobByIdHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        OrganisationAccessService organisationAccessService,
        IAppCache cache)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _organisationAccessService = organisationAccessService;
        _cache = cache;
    }

    public async Task<Result<JobDetailDto>> HandleAsync(
        GetJobByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<JobDetailDto>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var cacheKey = CacheKeys.JobDetail(query.JobId);

        var job = await _cache.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                var raw = await _dbContext.Jobs
                    .AsNoTracking()
                    .Where(x => x.Id == query.JobId)
                    .Select(x => new
                    {
                        x.Id,
                        x.OwnedByOrganisationId,
                        OwnedByOrganisationName = x.OwnedByOrganisation.Name,
                        x.ManagedByOrganisationId,
                        ManagedByOrganisationName = x.ManagedByOrganisation != null ? x.ManagedByOrganisation.Name : null,
                        x.ManagedByUserId,
                        x.OrganisationRecruitmentPartnershipId,
                        x.CreatedByType,
                        x.Status,
                        x.StatusReason,
                        x.Visibility,
                        x.Title,
                        x.ReferenceCode,
                        x.ExternalReference,
                        x.Department,
                        x.LocationText,
                        x.WorkplaceType,
                        x.EmploymentType,
                        x.Description,
                        x.Requirements,
                        x.Benefits,
                        x.Summary,
                        x.SalaryFrom,
                        x.SalaryTo,
                        x.SalaryCurrency,
                        x.PublishedUtc,
                        x.ApplyByUtc,
                        x.ClosedUtc,
                        x.SubmittedForApprovalUtc,
                        x.ApprovedByUserId,
                        x.ApprovedUtc,
                        x.ExternalApplicationUrl,
                        x.ApplicationEmail,
                        x.CreatedForUnclaimedCompany,
                        x.Category,
                        x.Regions,
                        x.Countries,
                        x.PostingExpiresUtc,
                        x.IncludeCompanyLogo,
                        x.IsHighlighted,
                        x.StickyUntilUtc,
                        x.AllowAutoMatch,
                        x.BenefitsTags,
                        x.ApplicationSpecialRequirements,
                        x.Keywords,
                        x.PoNumber,
                        x.ShortUrlCode,
                        x.HasCommission,
                        x.OteFrom,
                        x.OteTo,
                        x.IsImmediateStart,
                        x.VideoYouTubeId,
                        x.VideoVimeoId,
                        x.ScreeningQuestionsJson,
                        x.CreatedUtc,
                        x.CreatedByIdentityUserId,
                        ApplicationCount = x.Applications.Count
                    })
                    .SingleOrDefaultAsync(ct);

                if (raw is null) return null;

                return new JobDetailDto
                {
                    Id = raw.Id,
                    OwnedByOrganisationId = raw.OwnedByOrganisationId,
                    OwnedByOrganisationName = raw.OwnedByOrganisationName,
                    ManagedByOrganisationId = raw.ManagedByOrganisationId,
                    ManagedByOrganisationName = raw.ManagedByOrganisationName,
                    ManagedByUserId = raw.ManagedByUserId,
                    OrganisationRecruitmentPartnershipId = raw.OrganisationRecruitmentPartnershipId,
                    CreatedByType = raw.CreatedByType,
                    Status = raw.Status,
                    StatusReason = raw.StatusReason,
                    Visibility = raw.Visibility,
                    Title = raw.Title,
                    ReferenceCode = raw.ReferenceCode,
                    ExternalReference = raw.ExternalReference,
                    Department = raw.Department,
                    LocationText = raw.LocationText,
                    WorkplaceType = raw.WorkplaceType,
                    EmploymentType = raw.EmploymentType,
                    Description = raw.Description,
                    Requirements = raw.Requirements,
                    Benefits = raw.Benefits,
                    Summary = raw.Summary,
                    SalaryFrom = raw.SalaryFrom,
                    SalaryTo = raw.SalaryTo,
                    SalaryCurrency = raw.SalaryCurrency,
                    PublishedUtc = raw.PublishedUtc,
                    ApplyByUtc = raw.ApplyByUtc,
                    ClosedUtc = raw.ClosedUtc,
                    SubmittedForApprovalUtc = raw.SubmittedForApprovalUtc,
                    ApprovedByUserId = raw.ApprovedByUserId,
                    ApprovedUtc = raw.ApprovedUtc,
                    ExternalApplicationUrl = raw.ExternalApplicationUrl,
                    ApplicationEmail = raw.ApplicationEmail,
                    CreatedForUnclaimedCompany = raw.CreatedForUnclaimedCompany,
                    Category = raw.Category,
                    Regions = raw.Regions is not null
                        ? JsonSerializer.Deserialize<List<JobRegion>>(raw.Regions, _enumJson) ?? []
                        : [],
                    Countries = raw.Countries is not null
                        ? JsonSerializer.Deserialize<List<string>>(raw.Countries) ?? []
                        : [],
                    PostingExpiresUtc = raw.PostingExpiresUtc,
                    IncludeCompanyLogo = raw.IncludeCompanyLogo,
                    IsHighlighted = raw.IsHighlighted,
                    StickyUntilUtc = raw.StickyUntilUtc,
                    AllowAutoMatch = raw.AllowAutoMatch,
                    BenefitsTags = raw.BenefitsTags is not null
                        ? JsonSerializer.Deserialize<List<string>>(raw.BenefitsTags) ?? []
                        : [],
                    ApplicationSpecialRequirements = raw.ApplicationSpecialRequirements,
                    Keywords = raw.Keywords,
                    PoNumber = raw.PoNumber,
                    ShortUrlCode = raw.ShortUrlCode,
                    HasCommission = raw.HasCommission,
                    OteFrom = raw.OteFrom,
                    OteTo = raw.OteTo,
                    IsImmediateStart = raw.IsImmediateStart,
                    VideoYouTubeId = raw.VideoYouTubeId,
                    VideoVimeoId = raw.VideoVimeoId,
                    ScreeningQuestionsJson = raw.ScreeningQuestionsJson,
                    CreatedUtc = raw.CreatedUtc,
                    CreatedByIdentityUserId = raw.CreatedByIdentityUserId,
                    ApplicationCount = raw.ApplicationCount
                };
            },
            CacheTtl,
            cancellationToken);

        if (job is null)
        {
            return Result<JobDetailDto>.Failure(
                "jobs.not_found",
                "The requested job was not found.");
        }

        var canView = await _organisationAccessService.CanViewJobAsync(
            _currentUserAccessor.UserId,
            job.OwnedByOrganisationId,
            job.ManagedByOrganisationId,
            cancellationToken);

        if (!canView)
        {
            return Result<JobDetailDto>.Failure(
                "jobs.forbidden",
                "The current user cannot view this job.");
        }

        return Result<JobDetailDto>.Success(job);
    }

    private static readonly JsonSerializerOptions _enumJson = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
