using System.Security.Claims;
using Aethon.Api.Infrastructure;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Aethon.Shared.Organisations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Controllers;

[ApiController]
[Route("public")]
public sealed class PublicController : ControllerBase
{
    private readonly AethonDbContext _dbContext;

    public PublicController(AethonDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("jobs")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublishedJobs()
    {
        var jobs = await _dbContext.Jobs
            .AsNoTracking()
            .Where(x =>
                x.Status == JobStatus.Published &&
                x.OwnedByOrganisation.IsPublicProfileEnabled)
            .OrderByDescending(x => x.PublishedUtc)
            .Select(x => new PublicJobListItemDto
            {
                Id = x.Id,
                Title = x.Title,
                OrganisationName = x.OwnedByOrganisation.Name,
                OrganisationSlug = x.OwnedByOrganisation.Slug,
                OrganisationLogoUrl = x.OwnedByOrganisation.LogoUrl,
                Department = x.Department,
                LocationText = x.LocationText,
                WorkplaceType = x.WorkplaceType,
                EmploymentType = x.EmploymentType,
                SalaryFrom = x.SalaryFrom,
                SalaryTo = x.SalaryTo,
                SalaryCurrency = x.SalaryCurrency,
                PublishedUtc = x.PublishedUtc
            })
            .ToListAsync();

        return Ok(jobs);
    }

    [HttpGet("jobs/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublishedJob(Guid id)
    {
        var job = await _dbContext.Jobs
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.Status == JobStatus.Published &&
                x.OwnedByOrganisation.IsPublicProfileEnabled)
            .Select(x => new PublicJobDetailDto
            {
                Id = x.Id,
                Title = x.Title,
                Department = x.Department,
                LocationText = x.LocationText,
                WorkplaceType = x.WorkplaceType,
                EmploymentType = x.EmploymentType,
                Description = x.Description,
                Requirements = x.Requirements,
                Benefits = x.Benefits,
                SalaryFrom = x.SalaryFrom,
                SalaryTo = x.SalaryTo,
                SalaryCurrency = x.SalaryCurrency,
                PublishedUtc = x.PublishedUtc,
                Organisation = new PublicOrganisationProfileDto
                {
                    OrganisationId = x.OwnedByOrganisation.Id,
                    OrganisationType = x.OwnedByOrganisation.Type.ToString(),
                    Name = x.OwnedByOrganisation.Name,
                    Slug = x.OwnedByOrganisation.Slug,
                    LogoUrl = x.OwnedByOrganisation.LogoUrl,
                    WebsiteUrl = x.OwnedByOrganisation.WebsiteUrl,
                    Summary = x.OwnedByOrganisation.Summary,
                    PublicLocationText = x.OwnedByOrganisation.PublicLocationText,
                    PublicContactEmail = x.OwnedByOrganisation.PublicContactEmail,
                    PublicContactPhone = x.OwnedByOrganisation.PublicContactPhone
                }
            })
            .FirstOrDefaultAsync();

        return job is null ? NotFound() : Ok(job);
    }

    [HttpGet("organisations/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOrganisationBySlug(string slug)
    {
        var organisation = await _dbContext.Organisations
            .AsNoTracking()
            .Where(x => x.Slug == slug && x.IsPublicProfileEnabled)
            .Select(x => new PublicOrganisationProfileDto
            {
                OrganisationId = x.Id,
                OrganisationType = x.Type.ToString(),
                Name = x.Name,
                Slug = x.Slug,
                LogoUrl = x.LogoUrl,
                WebsiteUrl = x.WebsiteUrl,
                Summary = x.Summary,
                PublicLocationText = x.PublicLocationText,
                PublicContactEmail = x.PublicContactEmail,
                PublicContactPhone = x.PublicContactPhone
            })
            .FirstOrDefaultAsync();

        return organisation is null ? NotFound() : Ok(organisation);
    }

    [HttpPost("jobs/{id}/apply")]
    [Authorize]
    public async Task<IActionResult> ApplyForJob(Guid id, [FromBody] CreateJobApplicationRequestDto request)
    {
        var validationErrors = ApiValidationHelper.Validate(request);
        if (validationErrors.Count > 0)
        {
            return ValidationProblemResult(validationErrors);
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Forbid();
        }

        var job = await _dbContext.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.Status == JobStatus.Published &&
                x.OwnedByOrganisation.IsPublicProfileEnabled);

        if (job is null)
        {
            return NotFound();
        }

        var profile = await _dbContext.JobSeekerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile is null)
        {
            return ValidationProblemResult(new Dictionary<string, string[]>
            {
                ["Profile"] = ["A job seeker profile is required before applying."]
            });
        }

        var existing = await _dbContext.JobApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.JobId == id && x.UserId == userId);

        if (existing is not null)
        {
            return ValidationProblemResult(new Dictionary<string, string[]>
            {
                ["Job"] = ["You have already applied for this job."]
            });
        }

        var application = new JobApplication
        {
            Id = NewGuidId(),
            JobId = id,
            UserId = userId,
            Status = ApplicationStatus.Submitted,
            CoverLetter = string.IsNullOrWhiteSpace(request.CoverLetter) ? null : request.CoverLetter.Trim(),
            ResumeFileId = profile.ResumeFileId,
            SubmittedUtc = DateTime.UtcNow,
            LastStatusChangedUtc = DateTime.UtcNow,
            Source = string.IsNullOrWhiteSpace(request.Source) ? "AethonJobBoard" : request.Source.Trim(),
            Notes = null,
            CreatedUtc = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _dbContext.JobApplications.Add(application);
        await _dbContext.SaveChangesAsync();

        return Ok(new JobApplicationResultDto
        {
            Id = application.Id,
            JobId = application.JobId,
            Status = application.Status.ToString(),
            SubmittedUtc = application.SubmittedUtc
        });
    }

    private ObjectResult ValidationProblemResult(Dictionary<string, string[]> errors)
    {
        foreach (var error in errors)
        {
            ModelState.AddModelError(error.Key, string.Join(" ", error.Value));
        }

        return (ObjectResult)ValidationProblem(ModelState);
    }

    private static Guid NewGuidId()
    {
        return Guid.NewGuid();
    }
}