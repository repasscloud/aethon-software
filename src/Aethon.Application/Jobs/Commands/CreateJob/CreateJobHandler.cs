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
            .FirstOrDefaultAsync(x => x.Id == command.OwnedByOrganisationId, cancellationToken);

        if (organisation is null)
        {
            return Result<Guid>.Failure(
                "organisations.not_found",
                "The owning organisation was not found.");
        }

        var canCreate = await _organisationAccessService.CanCreateJobsAsync(
            currentUserId,
            command.OwnedByOrganisationId,
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
            OwnedByOrganisationId = command.OwnedByOrganisationId,
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
            Summary = Normalize(command.Summary),
            Department = Normalize(command.Department),
            LocationText = Normalize(command.LocationText),
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
            CreatedUtc = utcNow,
            CreatedByUserId = currentUserId
        };

        _dbContext.Jobs.Add(job);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(job.Id);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
