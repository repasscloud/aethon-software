using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Application.Organisations.Services;
using Aethon.Data;
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

    public async Task<Result<JobDetailModel>> HandleAsync(
        GetJobByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || !_currentUserAccessor.UserId.HasValue)
        {
            return Result<JobDetailModel>.Failure("auth.unauthenticated", "The current user is not authenticated.");
        }

        var job = await _dbContext.Jobs
            .AsNoTracking()
            .Where(x => x.Id == query.JobId)
            .Select(x => new JobDetailModel
            {
                Id = x.Id,
                OwnedByOrganisationId = x.OwnedByOrganisationId,
                ManagedByOrganisationId = x.ManagedByOrganisationId,
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
            .FirstOrDefaultAsync(cancellationToken);

        if (job is null)
        {
            return Result<JobDetailModel>.Failure("jobs.not_found", "The requested job was not found.");
        }

        var canView = await _organisationAccessService.CanViewJobAsync(
            _currentUserAccessor.UserId.Value,
            job.OwnedByOrganisationId,
            job.ManagedByOrganisationId,
            cancellationToken);

        if (!canView)
        {
            return Result<JobDetailModel>.Failure("jobs.forbidden", "The current user cannot view this job.");
        }

        return Result<JobDetailModel>.Success(job);
    }
}
