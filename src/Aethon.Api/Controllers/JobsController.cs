using System.Security.Claims;
using Aethon.Api.Infrastructure;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Auth;
using Aethon.Shared.Enums;
using Aethon.Shared.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Controllers;

[ApiController]
[Authorize]
[Route("jobs")]
public sealed class JobsController : ControllerBase
{
    private readonly AethonDbContext _dbContext;

    public JobsController(AethonDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("my-org")]
    public async Task<IActionResult> GetMyOrganisationJobs()
    {
        var organisationId = User.FindFirstValue(AppClaimTypes.OrganisationId);
        var organisationType = User.FindFirstValue(AppClaimTypes.OrganisationType);

        if (string.IsNullOrWhiteSpace(organisationId) || !string.Equals(organisationType, "company", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var jobs = await _dbContext.Jobs
            .AsNoTracking()
            .Where(x => x.OwnedByOrganisationId == organisationId)
            .OrderByDescending(x => x.CreatedUtc)
            .Select(x => new JobListItemDto
            {
                Id = x.Id,
                Title = x.Title,
                Department = x.Department,
                LocationText = x.LocationText,
                WorkplaceType = x.WorkplaceType,
                EmploymentType = x.EmploymentType,
                Status = x.Status,
                SalaryFrom = x.SalaryFrom,
                SalaryTo = x.SalaryTo,
                SalaryCurrency = x.SalaryCurrency,
                CreatedUtc = x.CreatedUtc,
                PublishedUtc = x.PublishedUtc
            })
            .ToListAsync();

        return Ok(jobs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetJob([FromRoute] string id)
    {
        var organisationId = User.FindFirstValue(AppClaimTypes.OrganisationId);
        var organisationType = User.FindFirstValue(AppClaimTypes.OrganisationType);

        if (string.IsNullOrWhiteSpace(organisationId) || !string.Equals(organisationType, "company", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var job = await _dbContext.Jobs
            .AsNoTracking()
            .Where(x => x.Id == id && x.OwnedByOrganisationId == organisationId)
            .Select(x => new JobDetailDto
            {
                Id = x.Id,
                OrganisationId = x.OwnedByOrganisationId,
                OrganisationName = x.OwnedByOrganisation.Name,
                Title = x.Title,
                Department = x.Department,
                LocationText = x.LocationText,
                WorkplaceType = x.WorkplaceType,
                EmploymentType = x.EmploymentType,
                Status = x.Status,
                Description = x.Description,
                Requirements = x.Requirements,
                Benefits = x.Benefits,
                SalaryFrom = x.SalaryFrom,
                SalaryTo = x.SalaryTo,
                SalaryCurrency = x.SalaryCurrency,
                CreatedByUserId = x.CreatedByIdentityUserId.ToString(),
                CreatedUtc = x.CreatedUtc,
                PublishedUtc = x.PublishedUtc,
                ClosedUtc = x.ClosedUtc
            })
            .FirstOrDefaultAsync();

        return job is null ? NotFound() : Ok(job);
    }

    [HttpPost]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobRequestDto request)
    {
        var validationErrors = ApiValidationHelper.Validate(request);
        if (validationErrors.Count > 0)
        {
            return ValidationProblem(validationErrors);
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var organisationId = User.FindFirstValue(AppClaimTypes.OrganisationId);
        var organisationType = User.FindFirstValue(AppClaimTypes.OrganisationType);

        if (!Guid.TryParse(userIdValue, out var userId) ||
            string.IsNullOrWhiteSpace(organisationId) ||
            !string.Equals(organisationType, "company", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var membership = await _dbContext.OrganisationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.OrganisationId == organisationId &&
                x.UserId == userId &&
                x.Status == MembershipStatus.Active);

        if (membership is null)
        {
            return Forbid();
        }

        var job = new Job
        {
            Id = Guid.NewGuid().ToString("N"),
            OwnedByOrganisationId = organisationId,
            ManagedByOrganisationId = null,
            CompanyRecruiterRelationshipId = null,
            CreatedByIdentityUserId = userId,
            CreatedByType = JobCreatedByType.CompanyUser,
            Status = request.Status!.Value,
            Visibility = request.Status.Value == JobStatus.Published ? JobVisibility.Public : JobVisibility.Private,
            Title = request.Title.Trim(),
            Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim(),
            LocationText = string.IsNullOrWhiteSpace(request.LocationText) ? null : request.LocationText.Trim(),
            WorkplaceType = request.WorkplaceType!.Value,
            EmploymentType = request.EmploymentType!.Value,
            Description = request.Description.Trim(),
            Requirements = string.IsNullOrWhiteSpace(request.Requirements) ? null : request.Requirements.Trim(),
            Benefits = string.IsNullOrWhiteSpace(request.Benefits) ? null : request.Benefits.Trim(),
            SalaryFrom = request.SalaryFrom,
            SalaryTo = request.SalaryTo,
            SalaryCurrency = request.SalaryCurrency,
            PublishedUtc = request.Status.Value == JobStatus.Published ? DateTime.UtcNow : null,
            ClosedUtc = request.Status.Value == JobStatus.Closed ? DateTime.UtcNow : null,
            CreatedForUnclaimedCompany = false,
            CreatedUtc = DateTime.UtcNow,
            CreatedByUserId = userId.ToString()
        };

        _dbContext.Jobs.Add(job);
        await _dbContext.SaveChangesAsync();

        return Ok(new JobDetailDto
        {
            Id = job.Id,
            OrganisationId = organisationId,
            OrganisationName = User.FindFirstValue(AppClaimTypes.OrganisationName) ?? "",
            Title = job.Title,
            Department = job.Department,
            LocationText = job.LocationText,
            WorkplaceType = job.WorkplaceType,
            EmploymentType = job.EmploymentType,
            Status = job.Status,
            Description = job.Description,
            Requirements = job.Requirements,
            Benefits = job.Benefits,
            SalaryFrom = job.SalaryFrom,
            SalaryTo = job.SalaryTo,
            SalaryCurrency = job.SalaryCurrency,
            CreatedByUserId = userId.ToString(),
            CreatedUtc = job.CreatedUtc,
            PublishedUtc = job.PublishedUtc,
            ClosedUtc = job.ClosedUtc
        });
    }

    private ObjectResult ValidationProblem(Dictionary<string, string[]> errors)
    {
        foreach (var error in errors)
        {
            ModelState.AddModelError(error.Key, string.Join(" ", error.Value));
        }

        return (ObjectResult)ValidationProblem(ModelState);
    }
}