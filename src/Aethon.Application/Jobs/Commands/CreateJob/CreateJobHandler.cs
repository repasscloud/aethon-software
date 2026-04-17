using System.Text.Json;
using System.Text.Json.Serialization;
using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Application.Organisations.Services;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Jobs.Commands.CreateJob;

public sealed class CreateJobHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly OrganisationAccessService _organisationAccessService;

    public CreateJobHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IDateTimeProvider dateTimeProvider,
        OrganisationAccessService organisationAccessService)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _dateTimeProvider = dateTimeProvider;
        _organisationAccessService = organisationAccessService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateJobCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<Guid>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var currentUserId = _currentUserAccessor.UserId;

        // If OwnedByOrganisationId is not supplied by the client, derive it from the
        // authenticated user's active organisation membership (company users creating
        // jobs for their own organisation).
        var ownedByOrgId = command.OwnedByOrganisationId;
        if (ownedByOrgId == Guid.Empty)
        {
            var membership = await _dbContext.Set<OrganisationMembership>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    m => m.UserId == currentUserId && m.Status == MembershipStatus.Active,
                    cancellationToken);

            if (membership is null)
            {
                return Result<Guid>.Failure(
                    "organisations.not_member",
                    "You are not a member of any organisation. Cannot create a job.");
            }

            ownedByOrgId = membership.OrganisationId;
        }

        if (string.IsNullOrWhiteSpace(command.Title))
        {
            return Result<Guid>.Failure(
                "jobs.title_required",
                "Job title is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Description))
        {
            return Result<Guid>.Failure(
                "jobs.description_required",
                "Job description is required.");
        }

        if (command.SalaryFrom.HasValue && command.SalaryTo.HasValue && command.SalaryFrom > command.SalaryTo)
        {
            return Result<Guid>.Failure(
                "jobs.salary_range_invalid",
                "SalaryFrom must be less than or equal to SalaryTo.");
        }

        var organisation = await _dbContext.Organisations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == ownedByOrgId, cancellationToken);

        if (organisation is null)
        {
            return Result<Guid>.Failure(
                "organisations.not_found",
                "The owning organisation was not found.");
        }

        var canCreate = await _organisationAccessService.CanCreateJobsAsync(
            currentUserId,
            ownedByOrgId,
            cancellationToken);

        if (!canCreate)
        {
            return Result<Guid>.Failure(
                "jobs.forbidden",
                "The current user cannot create jobs for this organisation.");
        }

        if (command.ManagedByOrganisationId.HasValue)
        {
            var managingOrganisationExists = await _dbContext.Organisations
                .AsNoTracking()
                .AnyAsync(x => x.Id == command.ManagedByOrganisationId.Value, cancellationToken);

            if (!managingOrganisationExists)
            {
                return Result<Guid>.Failure(
                    "jobs.managing_organisation_not_found",
                    "The managing organisation was not found.");
            }
        }

        if (command.OrganisationRecruitmentPartnershipId.HasValue)
        {
            var partnershipExists = await _dbContext.OrganisationRecruitmentPartnerships
                .AsNoTracking()
                .AnyAsync(x => x.Id == command.OrganisationRecruitmentPartnershipId.Value, cancellationToken);

            if (!partnershipExists)
            {
                return Result<Guid>.Failure(
                    "jobs.partnership_not_found",
                    "The recruitment partnership was not found.");
            }
        }

        var utcNow = _dateTimeProvider.UtcNow;
        var requiresApproval =
            command.ManagedByOrganisationId.HasValue &&
            command.ManagedByOrganisationId.Value != command.OwnedByOrganisationId;

        var job = new Job
        {
            Id = Guid.NewGuid(),
            OwnedByOrganisationId = ownedByOrgId,
            ManagedByOrganisationId = command.ManagedByOrganisationId,
            ManagedByUserId = command.ManagedByUserId,
            OrganisationRecruitmentPartnershipId = command.OrganisationRecruitmentPartnershipId,
            CreatedByIdentityUserId = currentUserId,
            CreatedByUser = null!,
            CreatedByType = command.CreatedByType,
            Status = requiresApproval ? JobStatus.PendingCompanyApproval : JobStatus.Draft,
            Visibility = command.Visibility,
            Title = command.Title.Trim(),
            Description = command.Description.Trim(),
            Summary = string.IsNullOrWhiteSpace(command.Summary) ? null : command.Summary.Trim(),
            Department = Normalize(command.Department),
            LocationText = Normalize(command.LocationText),
            LocationCity = Normalize(command.LocationCity),
            LocationState = Normalize(command.LocationState),
            LocationCountry = Normalize(command.LocationCountry),
            LocationCountryCode = Normalize(command.LocationCountryCode),
            LocationLatitude = command.LocationLatitude,
            LocationLongitude = command.LocationLongitude,
            LocationPlaceId = Normalize(command.LocationPlaceId),
            WorkplaceType = command.WorkplaceType,
            EmploymentType = command.EmploymentType,
            Requirements = Normalize(command.Requirements),
            Benefits = Normalize(command.Benefits),
            ReferenceCode = Normalize(command.ReferenceCode),
            ExternalReference = Normalize(command.ExternalReference),
            SalaryFrom = command.SalaryFrom,
            SalaryTo = command.SalaryTo,
            SalaryCurrency = command.SalaryCurrency,
            ApplyByUtc = command.ApplyByUtc,
            ExternalApplicationUrl = Normalize(command.ExternalApplicationUrl),
            ApplicationEmail = Normalize(command.ApplicationEmail),
            CreatedForUnclaimedCompany = command.CreatedForUnclaimedCompany,
            Category = command.Category,
            Regions = command.Regions.Count > 0 ? JsonSerializer.Serialize(command.Regions, _enumJson) : null,
            Countries = command.Countries.Count > 0 ? JsonSerializer.Serialize(command.Countries) : null,
            PostingExpiresUtc = command.PostingExpiresUtc,
            PostingTier = command.PostingTier,
            IncludeCompanyLogo = command.IncludeCompanyLogo,
            IsHighlighted = command.IsHighlighted,
            HighlightColour = command.HighlightColour,
            HasAiCandidateMatching = command.HasAiCandidateMatching,
            StickyUntilUtc = command.StickyUntilUtc,
            AllowAutoMatch = command.AllowAutoMatch,
            BenefitsTags = command.BenefitsTags.Count > 0
                ? JsonSerializer.Serialize(command.BenefitsTags)
                : null,
            ApplicationSpecialRequirements = Normalize(command.ApplicationSpecialRequirements),
            HasCommission = command.HasCommission,
            OteFrom = command.OteFrom,
            OteTo = command.OteTo,
            IsImmediateStart = command.IsImmediateStart,
            VideoYouTubeId = Normalize(command.VideoYouTubeId),
            VideoVimeoId = Normalize(command.VideoVimeoId),
            Keywords = Normalize(command.Keywords),
            PoNumber = Normalize(command.PoNumber),
            ScreeningQuestionsJson = command.ScreeningQuestionsJson,
            CreatedUtc = utcNow,
            CreatedByUserId = currentUserId
        };

        _dbContext.Jobs.Add(job);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(job.Id);
    }

    private static readonly JsonSerializerOptions _enumJson = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
