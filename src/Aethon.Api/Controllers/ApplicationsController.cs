using System.Security.Claims;
using Aethon.Api.Infrastructure;
using Aethon.Data;
using Aethon.Shared.Auth;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Controllers;

[ApiController]
[Authorize]
public sealed class ApplicationsController : ControllerBase
{
    private readonly AethonDbContext _dbContext;

    public ApplicationsController(AethonDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("jobs/{jobId}/applications")]
    public async Task<IActionResult> GetJobApplications(string jobId)
    {
        var access = await GetJobAccessAsync(jobId);
        if (!access.Succeeded)
        {
            return access.Result!;
        }

        var items = await _dbContext.JobApplications
            .AsNoTracking()
            .Where(x => x.JobId == jobId)
            .OrderByDescending(x => x.SubmittedUtc)
            .Select(x => new EmployerJobApplicationListItemDto
            {
                Id = x.Id,
                JobId = x.JobId,
                ApplicantUserId = x.UserId.ToString(),
                ApplicantDisplayName = x.User.DisplayName,
                ApplicantEmail = x.User.Email ?? "",
                Status = x.Status.ToString(),
                Source = x.Source,
                SubmittedUtc = x.SubmittedUtc,
                LastStatusChangedUtc = x.LastStatusChangedUtc
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("applications/{id}")]
    public async Task<IActionResult> GetApplication(string id)
    {
        var application = await _dbContext.JobApplications
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.JobId,
                x.Job.Title,
                x.Job.OwnedByOrganisationId,
                x.Job.ManagedByOrganisationId,
                ApplicantUserId = x.UserId,
                ApplicantDisplayName = x.User.DisplayName,
                ApplicantEmail = x.User.Email,
                Status = x.Status,
                x.CoverLetter,
                x.ResumeFileId,
                x.Source,
                x.Notes,
                x.SubmittedUtc,
                x.LastStatusChangedUtc
            })
            .FirstOrDefaultAsync();

        if (application is null)
        {
            return NotFound();
        }

        var access = await GetJobAccessAsync(
            application.JobId,
            application.OwnedByOrganisationId,
            application.ManagedByOrganisationId);

        if (!access.Succeeded)
        {
            return access.Result!;
        }

        return Ok(new EmployerJobApplicationDetailDto
        {
            Id = application.Id,
            JobId = application.JobId,
            JobTitle = application.Title,
            ApplicantUserId = application.ApplicantUserId.ToString(),
            ApplicantDisplayName = application.ApplicantDisplayName,
            ApplicantEmail = application.ApplicantEmail ?? "",
            Status = application.Status.ToString(),
            CoverLetter = application.CoverLetter,
            ResumeFileId = application.ResumeFileId,
            Source = application.Source,
            Notes = application.Notes,
            SubmittedUtc = application.SubmittedUtc,
            LastStatusChangedUtc = application.LastStatusChangedUtc
        });
    }

    [HttpGet("applications/status-options")]
    public IActionResult GetStatusOptions()
    {
        return Ok(Enum.GetNames<ApplicationStatus>());
    }

    [HttpPost("applications/{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateJobApplicationStatusRequestDto request)
    {
        var validationErrors = ApiValidationHelper.Validate(request);
        if (validationErrors.Count > 0)
        {
            return ValidationProblemResult(validationErrors);
        }

        var application = await _dbContext.JobApplications
            .Include(x => x.Job)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (application is null)
        {
            return NotFound();
        }

        var access = await GetJobAccessAsync(
            application.JobId,
            application.Job.OwnedByOrganisationId,
            application.Job.ManagedByOrganisationId);

        if (!access.Succeeded)
        {
            return access.Result!;
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Forbid();
        }

        if (!Enum.TryParse<ApplicationStatus>(request.Status, true, out var parsedStatus))
        {
            return ValidationProblemResult(new Dictionary<string, string[]>
            {
                [nameof(UpdateJobApplicationStatusRequestDto.Status)] = ["Invalid application status."]
            });
        }

        application.Status = parsedStatus;
        application.Notes = string.IsNullOrWhiteSpace(request.Notes)
            ? application.Notes
            : request.Notes.Trim();
        application.LastStatusChangedUtc = DateTime.UtcNow;
        application.UpdatedUtc = DateTime.UtcNow;
        application.UpdatedByUserId = userId.ToString();

        await _dbContext.SaveChangesAsync();

        return Ok(new EmployerJobApplicationDetailDto
        {
            Id = application.Id,
            JobId = application.JobId,
            JobTitle = application.Job.Title,
            ApplicantUserId = application.UserId.ToString(),
            ApplicantDisplayName = application.User.DisplayName,
            ApplicantEmail = application.User.Email ?? "",
            Status = application.Status.ToString(),
            CoverLetter = application.CoverLetter,
            ResumeFileId = application.ResumeFileId,
            Source = application.Source,
            Notes = application.Notes,
            SubmittedUtc = application.SubmittedUtc,
            LastStatusChangedUtc = application.LastStatusChangedUtc
        });
    }

    private async Task<JobAccessResult> GetJobAccessAsync(
        string jobId,
        string? ownedByOrganisationId = null,
        string? managedByOrganisationId = null)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var organisationId = User.FindFirstValue(AppClaimTypes.OrganisationId);
        var organisationType = User.FindFirstValue(AppClaimTypes.OrganisationType);

        if (!Guid.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(organisationId))
        {
            return JobAccessResult.Forbidden();
        }

        if (ownedByOrganisationId is null)
        {
            var job = await _dbContext.Jobs
                .AsNoTracking()
                .Where(x => x.Id == jobId)
                .Select(x => new
                {
                    x.OwnedByOrganisationId,
                    x.ManagedByOrganisationId
                })
                .FirstOrDefaultAsync();

            if (job is null)
            {
                return JobAccessResult.NotFound();
            }

            ownedByOrganisationId = job.OwnedByOrganisationId;
            managedByOrganisationId = job.ManagedByOrganisationId;
        }

        var hasMembership = await _dbContext.OrganisationMemberships
            .AsNoTracking()
            .AnyAsync(x =>
                x.OrganisationId == organisationId &&
                x.UserId == userId &&
                x.Status == MembershipStatus.Active);

        if (!hasMembership)
        {
            return JobAccessResult.Forbidden();
        }

        if (string.Equals(organisationType, "company", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(organisationId, ownedByOrganisationId, StringComparison.Ordinal))
        {
            return JobAccessResult.Success();
        }

        if (string.Equals(organisationType, "recruiter", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(organisationId, managedByOrganisationId, StringComparison.Ordinal))
        {
            return JobAccessResult.Success();
        }

        return JobAccessResult.Forbidden();
    }

    private ObjectResult ValidationProblemResult(Dictionary<string, string[]> errors)
    {
        foreach (var error in errors)
        {
            ModelState.AddModelError(error.Key, string.Join(" ", error.Value));
        }

        return (ObjectResult)ValidationProblem(ModelState);
    }

    private sealed class JobAccessResult
    {
        public bool Succeeded { get; init; }
        public IActionResult? Result { get; init; }

        public static JobAccessResult Success() => new() { Succeeded = true };
        public static JobAccessResult Forbidden() => new() { Result = new ForbidResult() };
        public static JobAccessResult NotFound() => new() { Result = new NotFoundResult() };
    }
}