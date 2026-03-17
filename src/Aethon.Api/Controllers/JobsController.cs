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
        var access = await GetEmployerAccessAsync();

        if (!access.Succeeded)
        {
            return Forbid();
        }

        var jobs = await _dbContext.Jobs
            .AsNoTracking()
            .Where(x => x.OwnedByOrganisationId == access.OrganisationId)
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
    public async Task<IActionResult> GetJob([FromRoute] Guid id)
    {
        var access = await GetEmployerAccessAsync();

        if (!access.Succeeded)
        {
            return Forbid();
        }

        var job = await _dbContext.Jobs
            .AsNoTracking()
            .Where(x => x.Id == id && x.OwnedByOrganisationId == access.OrganisationId)
            .Select(x => new JobDetailDto
            {
                Id = x.Id,
                CompanyOrganisationId = x.OwnedByOrganisationId,
                CompanyOrganisationName = x.OwnedByOrganisation != null
                    ? x.OwnedByOrganisation.Name
                    : null,
                ManagedByRecruiterOrganisationId = x.ManagedByOrganisationId,
                ManagedByRecruiterOrganisationName = x.ManagedByOrganisation != null
                    ? x.ManagedByOrganisation.Name
                    : null,
                Title = x.Title,
                Department = x.Department,
                Location = x.LocationText,
                WorkplaceType = x.WorkplaceType,
                EmploymentType = x.EmploymentType,
                Status = x.Status,
                Description = x.Description,
                Requirements = x.Requirements,
                Benefits = x.Benefits,
                SalaryMin = x.SalaryFrom,
                SalaryMax = x.SalaryTo,
                SalaryCurrency = x.SalaryCurrency,
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

        var access = await GetEmployerAccessAsync();
        if (!access.Succeeded)
        {
            return access.ForbiddenResult!;
        }

        var status = request.Status ?? JobStatus.Draft;
        var workplaceType = request.WorkplaceType ?? WorkplaceType.OnSite;
        var employmentType = request.EmploymentType ?? EmploymentType.FullTime;

        var job = new Job
        {
            Id = Guid.NewGuid(),
            OwnedByOrganisationId = access.OrganisationId!.Value,
            ManagedByOrganisationId = null,
            CompanyRecruiterRelationshipId = null,
            CreatedByIdentityUserId = access.UserId!.Value,
            CreatedByType = JobCreatedByType.CompanyUser,
            Status = status,
            Visibility = status == JobStatus.Published ? JobVisibility.Public : JobVisibility.Private,
            Title = request.Title.Trim(),
            Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim(),
            LocationText = string.IsNullOrWhiteSpace(request.LocationText) ? null : request.LocationText.Trim(),
            WorkplaceType = workplaceType,
            EmploymentType = employmentType,
            Description = request.Description.Trim(),
            Requirements = string.IsNullOrWhiteSpace(request.Requirements) ? null : request.Requirements.Trim(),
            Benefits = string.IsNullOrWhiteSpace(request.Benefits) ? null : request.Benefits.Trim(),
            SalaryFrom = request.SalaryFrom,
            SalaryTo = request.SalaryTo,
            SalaryCurrency = request.SalaryCurrency,
            PublishedUtc = status == JobStatus.Published ? DateTime.UtcNow : null,
            ClosedUtc = status == JobStatus.Closed ? DateTime.UtcNow : null,
            CreatedForUnclaimedCompany = false,
            CreatedUtc = DateTime.UtcNow,
            CreatedByUserId = access.UserId
        };

        _dbContext.Jobs.Add(job);
        await _dbContext.SaveChangesAsync();

        return Ok(await BuildJobDetailAsync(job.Id, access.OrganisationId.Value));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateJob([FromRoute] Guid id, [FromBody] UpdateJobRequestDto request)
    {
        var validationErrors = ApiValidationHelper.Validate(request);
        if (validationErrors.Count > 0)
        {
            return ValidationProblem(validationErrors);
        }

        var access = await GetEmployerAccessAsync();
        if (!access.Succeeded)
        {
            return access.ForbiddenResult!;
        }

        var job = await _dbContext.Jobs
            .FirstOrDefaultAsync(x => x.Id == id && x.OwnedByOrganisationId == access.OrganisationId);

        if (job is null)
        {
            return NotFound();
        }

        job.Title = request.Title.Trim();
        job.Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim();
        job.LocationText = string.IsNullOrWhiteSpace(request.LocationText) ? null : request.LocationText.Trim();
        job.WorkplaceType = request.WorkplaceType ?? WorkplaceType.OnSite;
        job.EmploymentType = request.EmploymentType ?? EmploymentType.FullTime;
        job.Description = request.Description.Trim();
        job.Requirements = string.IsNullOrWhiteSpace(request.Requirements) ? null : request.Requirements.Trim();
        job.Benefits = string.IsNullOrWhiteSpace(request.Benefits) ? null : request.Benefits.Trim();
        job.SalaryFrom = request.SalaryFrom;
        job.SalaryTo = request.SalaryTo;
        job.SalaryCurrency = request.SalaryCurrency;
        job.UpdatedUtc = DateTime.UtcNow;
        job.UpdatedByUserId = access.UserId!;

        await _dbContext.SaveChangesAsync();

        return Ok(await BuildJobDetailAsync(job.Id, access.OrganisationId!.Value));
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> PublishJob([FromRoute] Guid id)
    {
        var access = await GetEmployerAccessAsync();
        if (!access.Succeeded)
        {
            return access.ForbiddenResult!;
        }

        var job = await _dbContext.Jobs
            .FirstOrDefaultAsync(x => x.Id == id && x.OwnedByOrganisationId == access.OrganisationId);

        if (job is null)
        {
            return NotFound();
        }

        if (job.Status == JobStatus.Closed)
        {
            return ValidationProblem(new Dictionary<string, string[]>
            {
                ["Status"] = ["Closed jobs must be returned to draft before publishing."]
            });
        }

        job.Status = JobStatus.Published;
        job.Visibility = JobVisibility.Public;
        job.PublishedUtc ??= DateTime.UtcNow;
        job.ClosedUtc = null;
        job.UpdatedUtc = DateTime.UtcNow;
        job.UpdatedByUserId = access.UserId!;

        await _dbContext.SaveChangesAsync();

        return Ok(await BuildJobDetailAsync(job.Id, access.OrganisationId!.Value));
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseJob([FromRoute] Guid id)
    {
        var access = await GetEmployerAccessAsync();
        if (!access.Succeeded)
        {
            return access.ForbiddenResult!;
        }

        var job = await _dbContext.Jobs
            .FirstOrDefaultAsync(x => x.Id == id && x.OwnedByOrganisationId == access.OrganisationId);

        if (job is null)
        {
            return NotFound();
        }

        job.Status = JobStatus.Closed;
        job.Visibility = JobVisibility.Private;
        job.ClosedUtc = DateTime.UtcNow;
        job.UpdatedUtc = DateTime.UtcNow;
        job.UpdatedByUserId = access.UserId!;

        await _dbContext.SaveChangesAsync();

        return Ok(await BuildJobDetailAsync(job.Id, access.OrganisationId!.Value));
    }

    [HttpPost("{id}/return-to-draft")]
    public async Task<IActionResult> ReturnToDraft([FromRoute] Guid id)
    {
        var access = await GetEmployerAccessAsync();
        if (!access.Succeeded)
        {
            return access.ForbiddenResult!;
        }

        var job = await _dbContext.Jobs
            .FirstOrDefaultAsync(x => x.Id == id && x.OwnedByOrganisationId == access.OrganisationId);

        if (job is null)
        {
            return NotFound();
        }

        job.Status = JobStatus.Draft;
        job.Visibility = JobVisibility.Private;
        job.ClosedUtc = null;
        job.UpdatedUtc = DateTime.UtcNow;
        job.UpdatedByUserId = access.UserId!;

        await _dbContext.SaveChangesAsync();

        return Ok(await BuildJobDetailAsync(job.Id, access.OrganisationId!.Value));
    }

    [HttpGet("{jobId:guid}")]
    public async Task<ActionResult<JobDetailDto>> GetById(
        Guid jobId,
        [FromServices] AethonDbContext db,
        CancellationToken cancellationToken)
    {
        var result = await db.Jobs
            .AsNoTracking()
            .Where(x => x.Id == jobId)
            .Select(x => new JobDetailDto
            {
                Id = x.Id,
                CompanyOrganisationId = x.OwnedByOrganisationId,
                ManagedByRecruiterOrganisationName = x.ManagedByOrganisation != null
                    ? x.ManagedByOrganisation.Name
                    : null,
                Title = x.Title,
                Summary = x.Summary,
                Description = x.Description,
                Location = x.LocationText,
                SalaryMin = x.SalaryFrom,
                SalaryMax = x.SalaryTo,
                Status = x.Status,
                StatusReason = x.StatusReason,
                CreatedUtc = x.CreatedUtc,
                SubmittedForApprovalUtc = x.SubmittedForApprovalUtc,
                ApprovedUtc = x.ApprovedUtc,
                PublishedUtc = x.PublishedUtc
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    private async Task<JobDetailDto> BuildJobDetailAsync(Guid jobId, Guid organisationId)
    {
        return await _dbContext.Jobs
            .AsNoTracking()
            .Where(x => x.Id == jobId && x.OwnedByOrganisationId == organisationId)
            .Select(x => new JobDetailDto
            {
                Id = x.Id,
                CompanyOrganisationId = x.OwnedByOrganisationId,
                ManagedByRecruiterOrganisationId = x.ManagedByOrganisationId,
                ManagedByRecruiterOrganisationName = x.ManagedByOrganisation != null
                    ? x.ManagedByOrganisation.Name
                    : null,
                Title = x.Title,
                Department = x.Department,
                Location= x.LocationText,
                WorkplaceType = x.WorkplaceType,
                EmploymentType = x.EmploymentType,
                Status = x.Status,
                Description = x.Description,
                Requirements = x.Requirements,
                Benefits = x.Benefits,
                SalaryMin = x.SalaryFrom,
                SalaryMax = x.SalaryTo,
                SalaryCurrency = x.SalaryCurrency,
                CreatedUtc = x.CreatedUtc,
                PublishedUtc = x.PublishedUtc,
                ClosedUtc = x.ClosedUtc
            })
            .SingleAsync();
    }

    private async Task<EmployerAccessResult> GetEmployerAccessAsync()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var organisationIdValue = User.FindFirstValue(AppClaimTypes.OrganisationId);
        var organisationType = User.FindFirstValue(AppClaimTypes.OrganisationType);

        if (!Guid.TryParse(userIdValue, out var userId) ||
            !Guid.TryParse(organisationIdValue, out var organisationId) ||
            !string.Equals(organisationType, "company", StringComparison.OrdinalIgnoreCase))
        {
            return EmployerAccessResult.Forbidden();
        }

        var membership = await _dbContext.OrganisationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.OrganisationId == organisationId &&
                x.UserId == userId &&
                x.Status == MembershipStatus.Active);

        return membership is null
            ? EmployerAccessResult.Forbidden()
            : EmployerAccessResult.Success(userId, organisationId);
    }

    private ObjectResult ValidationProblem(Dictionary<string, string[]> errors)
    {
        foreach (var error in errors)
        {
            ModelState.AddModelError(error.Key, string.Join(" ", error.Value));
        }

        return (ObjectResult)ValidationProblem(ModelState);
    }

    private sealed class EmployerAccessResult
    {
        public bool Succeeded { get; init; }
        public Guid? UserId { get; init; }
        public Guid? OrganisationId { get; init; }
        public IActionResult? ForbiddenResult { get; init; }

        public static EmployerAccessResult Success(Guid userId, Guid organisationId) =>
            new()
            {
                Succeeded = true,
                UserId = userId,
                OrganisationId = organisationId
            };

        public static EmployerAccessResult Forbidden() =>
            new()
            {
                Succeeded = false,
                ForbiddenResult = new ForbidResult()
            };
    }
}
