using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Applications.Services;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Shared.Applications;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Application.Applications.Queries.GetApplicationById;

public sealed class GetApplicationByIdHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ApplicationAccessService _applicationAccessService;

    public GetApplicationByIdHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        ApplicationAccessService applicationAccessService)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _applicationAccessService = applicationAccessService;
    }

    public async Task<Result<ApplicationDetailDto>> HandleAsync(
        GetApplicationByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<ApplicationDetailDto>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        var currentUserId = _currentUserAccessor.UserId;

        var application = await _dbContext.JobApplications
            .AsNoTracking()
            .Where(x => x.Id == query.ApplicationId)
            .Select(x => new ApplicationDetailDto
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
                Resume = x.ResumeFileId.HasValue
                    ? x.User.JobSeekerProfile != null
                        ? x.User.JobSeekerProfile.Resumes
                            .Where(r => r.StoredFileId == x.ResumeFileId.Value && r.IsActive)
                            .Select(r => new ApplicationResumeDto
                            {
                                Id = r.Id,
                                StoredFileId = r.StoredFileId,
                                Name = r.Name,
                                Description = r.Description,
                                IsDefault = r.IsDefault,
                                OriginalFileName = r.StoredFile.OriginalFileName,
                                ContentType = r.StoredFile.ContentType,
                                LengthBytes = r.StoredFile.LengthBytes
                            })
                            .FirstOrDefault()
                        : null
                    : null,
                CoverLetter = x.CoverLetter,
                Source = x.Source ?? string.Empty,
                SubmittedUtc = x.SubmittedUtc,
                LastStatusChangedUtc = x.LastStatusChangedUtc,
                LastActivityUtc = x.LastActivityUtc,
                AssignedRecruiterUserId = x.AssignedRecruiterUserId,
                AssignedRecruiterDisplayName = x.AssignedRecruiterUser != null
                    ? x.AssignedRecruiterUser.DisplayName
                    : null,
                Rating = x.Rating,
                Recommendation = x.Recommendation,
                IsRejected = x.IsRejected,
                IsWithdrawn = x.IsWithdrawn,
                IsHired = x.IsHired
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (application is null)
        {
            return Result<ApplicationDetailDto>.Failure(
                "applications.not_found",
                "The requested application was not found.");
        }

        var canManage = await _applicationAccessService.CanManageApplicationAsync(
            currentUserId,
            query.ApplicationId,
            cancellationToken);

        var canViewOwn = application.ApplicantUserId == currentUserId;

        if (!canManage && !canViewOwn)
        {
            return Result<ApplicationDetailDto>.Failure(
                "applications.forbidden",
                "The current user cannot view this application.");
        }

        return Result<ApplicationDetailDto>.Success(application);
    }
}