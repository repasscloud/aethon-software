using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Common.Results;
using Aethon.Application.Organisations.Services;
using Aethon.Data;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Applications.Queries.GetApplicationById;

public sealed class GetApplicationByIdHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly OrganisationAccessService _organisationAccessService;

    public GetApplicationByIdHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        OrganisationAccessService organisationAccessService)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _organisationAccessService = organisationAccessService;
    }

    public async Task<Result<ApplicationDetailModel>> HandleAsync(
        GetApplicationByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || !_currentUserAccessor.UserId.HasValue)
        {
            return Result<ApplicationDetailModel>.Failure("auth.unauthenticated", "The current user is not authenticated.");
        }

        var application = await _dbContext.JobApplications
            .AsNoTracking()
            .Where(x => x.Id == query.ApplicationId)
            .Select(x => new ApplicationDetailModel
            {
                Id = x.Id,
                JobId = x.JobId,
                JobTitle = x.Job.Title,
                ApplicantUserId = x.UserId,
                ApplicantDisplayName = x.User.DisplayName,
                ApplicantEmail = x.User.Email ?? string.Empty,
                Status = x.Status,
                StatusReason = x.StatusReason,
                ResumeFileId = x.ResumeFileId,
                CoverLetter = x.CoverLetter,
                Source = x.Source ?? string.Empty,
                SubmittedUtc = x.SubmittedUtc,
                LastStatusChangedUtc = x.LastStatusChangedUtc,
                OwnedByOrganisationId = x.Job.OwnedByOrganisationId,
                ManagedByOrganisationId = x.Job.ManagedByOrganisationId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (application is null)
        {
            return Result<ApplicationDetailModel>.Failure("applications.not_found", "The requested application was not found.");
        }

        var canView = application.ApplicantUserId == _currentUserAccessor.UserId.Value ||
                      await _organisationAccessService.CanViewJobAsync(
                          _currentUserAccessor.UserId.Value,
                          application.OwnedByOrganisationId,
                          application.ManagedByOrganisationId,
                          cancellationToken);

        if (!canView)
        {
            return Result<ApplicationDetailModel>.Failure("applications.forbidden", "The current user cannot view this application.");
        }

        return Result<ApplicationDetailModel>.Success(application);
    }
}
