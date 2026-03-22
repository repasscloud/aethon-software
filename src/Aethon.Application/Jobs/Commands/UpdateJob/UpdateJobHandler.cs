using System.Text.Json;
using System.Text.Json.Serialization;
using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Application.Organisations.Services;
using Aethon.Data;
using Aethon.Shared.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Jobs.Commands.UpdateJob;

public sealed class UpdateJobHandler
{
    private readonly AethonDbContext _db;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly OrganisationAccessService _orgAccess;

    public UpdateJobHandler(
        AethonDbContext db,
        ICurrentUserAccessor currentUser,
        OrganisationAccessService orgAccess)
    {
        _db = db;
        _currentUser = currentUser;
        _orgAccess = orgAccess;
    }

    public async Task<Result> HandleAsync(Guid jobId, UpdateJobRequestDto request, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure("auth.unauthenticated", "Not authenticated.");

        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);

        if (job is null)
            return Result.Failure("jobs.not_found", "Job not found.");

        var canEdit = await _orgAccess.CanCreateJobsAsync(_currentUser.UserId, job.OwnedByOrganisationId, ct) ||
                      (job.ManagedByOrganisationId.HasValue &&
                       await _orgAccess.CanCreateJobsAsync(_currentUser.UserId, job.ManagedByOrganisationId.Value, ct));

        if (!canEdit)
            return Result.Failure("jobs.forbidden", "Insufficient permissions to edit this job.");

        job.Title = request.Title.Trim();
        job.Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim();
        job.Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim();
        job.LocationText = string.IsNullOrWhiteSpace(request.LocationText) ? null : request.LocationText.Trim();
        job.LocationCity = string.IsNullOrWhiteSpace(request.LocationCity) ? null : request.LocationCity.Trim();
        job.LocationState = string.IsNullOrWhiteSpace(request.LocationState) ? null : request.LocationState.Trim();
        job.LocationCountry = string.IsNullOrWhiteSpace(request.LocationCountry) ? null : request.LocationCountry.Trim();
        job.LocationCountryCode = string.IsNullOrWhiteSpace(request.LocationCountryCode) ? null : request.LocationCountryCode.Trim();
        job.LocationLatitude = request.LocationLatitude;
        job.LocationLongitude = request.LocationLongitude;
        job.LocationPlaceId = string.IsNullOrWhiteSpace(request.LocationPlaceId) ? null : request.LocationPlaceId.Trim();
        job.WorkplaceType = request.WorkplaceType!.Value;
        job.EmploymentType = request.EmploymentType!.Value;
        job.Description = request.Description.Trim();
        job.Requirements = string.IsNullOrWhiteSpace(request.Requirements) ? null : request.Requirements.Trim();
        job.Benefits = string.IsNullOrWhiteSpace(request.Benefits) ? null : request.Benefits.Trim();
        job.SalaryFrom = request.SalaryFrom;
        job.SalaryTo = request.SalaryTo;
        job.SalaryCurrency = request.SalaryCurrency;
        job.ExternalApplicationUrl = string.IsNullOrWhiteSpace(request.ExternalApplicationUrl) ? null : request.ExternalApplicationUrl.Trim();
        job.ApplicationEmail = string.IsNullOrWhiteSpace(request.ApplicationEmail) ? null : request.ApplicationEmail.Trim();
        job.Visibility = request.Visibility;
        job.Category = request.Category;
        job.Regions = request.Regions.Count > 0 ? JsonSerializer.Serialize(request.Regions, _enumJson) : null;
        job.Countries = request.Countries.Count > 0 ? JsonSerializer.Serialize(request.Countries) : null;
        job.PostingExpiresUtc = request.PostingExpiresUtc;
        job.IncludeCompanyLogo = request.IncludeCompanyLogo;
        job.IsHighlighted = request.IsHighlighted;
        job.StickyUntilUtc = request.StickyUntilUtc;
        job.AllowAutoMatch = request.AllowAutoMatch;
        job.BenefitsTags = request.BenefitsTags.Count > 0
            ? JsonSerializer.Serialize(request.BenefitsTags)
            : null;
        job.ApplicationSpecialRequirements = string.IsNullOrWhiteSpace(request.ApplicationSpecialRequirements)
            ? null
            : request.ApplicationSpecialRequirements.Trim();
        job.HasCommission = request.HasCommission;
        job.OteFrom = request.OteFrom;
        job.OteTo = request.OteTo;
        job.IsImmediateStart = request.IsImmediateStart;
        job.VideoYouTubeId = string.IsNullOrWhiteSpace(request.VideoYouTubeId) ? null : request.VideoYouTubeId.Trim();
        job.VideoVimeoId = string.IsNullOrWhiteSpace(request.VideoVimeoId) ? null : request.VideoVimeoId.Trim();
        job.Keywords = string.IsNullOrWhiteSpace(request.Keywords) ? null : request.Keywords.Trim();
        job.PoNumber = string.IsNullOrWhiteSpace(request.PoNumber) ? null : request.PoNumber.Trim();
        job.ScreeningQuestionsJson = request.ScreeningQuestionsJson;
        job.UpdatedUtc = DateTime.UtcNow;
        job.UpdatedByUserId = _currentUser.UserId;

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static readonly JsonSerializerOptions _enumJson = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
