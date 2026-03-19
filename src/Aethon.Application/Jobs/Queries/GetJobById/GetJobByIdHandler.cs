using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Application.Organisations.Services;
using Aethon.Data;
using Aethon.Shared.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Jobs.Queries.GetJobById;

public sealed class GetJobByIdHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly OrganisationAccessService _organisationAccessService;

    public GetJobByIdHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        OrganisationAccessService organisationAccessService)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _organisationAccessService = organisationAccessService;
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

        var job = await _dbContext.Jobs
            .AsNoTracking()
            .Where(x => x.Id == query.JobId)
            .Select(x => new JobDetailDto
            {
                Id = x.Id,
                OwnedByOrganisationId = x.OwnedByOrganisationId,
                OwnedByOrganisationName = x.OwnedByOrganisation.Name,
                ManagedByOrganisationId = x.ManagedByOrganisationId,
                ManagedByOrganisationName = x.ManagedByOrganisation != null
                    ? x.ManagedByOrganisation.Name
                    : null,
                ManagedByUserId = x.ManagedByUserId,
                OrganisationRecruitmentPartnershipId = x.OrganisationRecruitmentPartnershipId,
                CreatedByType = x.CreatedByType,
                Status = x.Status,
                StatusReason = x.StatusReason,
                Visibility = x.Visibility,
                Title = x.Title,
                ReferenceCode = x.ReferenceCode,
                ExternalReference = x.ExternalReference,
                Department = x.Department,
                LocationText = x.LocationText,
                WorkplaceType = x.WorkplaceType,
                EmploymentType = x.EmploymentType,
                Description = x.Description,
                Requirements = x.Requirements,
                Benefits = x.Benefits,
                Summary = x.Summary,
                SalaryFrom = x.SalaryFrom,
                SalaryTo = x.SalaryTo,
                SalaryCurrency = x.SalaryCurrency,
                PublishedUtc = x.PublishedUtc,
                ApplyByUtc = x.ApplyByUtc,
                ClosedUtc = x.ClosedUtc,
                SubmittedForApprovalUtc = x.SubmittedForApprovalUtc,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedUtc = x.ApprovedUtc,
                ExternalApplicationUrl = x.ExternalApplicationUrl,
                ApplicationEmail = x.ApplicationEmail,
                CreatedForUnclaimedCompany = x.CreatedForUnclaimedCompany,
                CreatedUtc = x.CreatedUtc,
                CreatedByIdentityUserId = x.CreatedByIdentityUserId,
                ApplicationCount = x.Applications.Count
            })
            .SingleOrDefaultAsync(cancellationToken);

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
}
