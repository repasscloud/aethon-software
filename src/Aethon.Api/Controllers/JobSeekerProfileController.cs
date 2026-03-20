using System.Security.Claims;
using Aethon.Api.Infrastructure;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Files;
using Aethon.Shared.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Controllers;

[ApiController]
[Authorize]
[Route("jobseeker/profile")]
public sealed class JobSeekerProfileController : ControllerBase
{
    private readonly AethonDbContext _dbContext;

    public JobSeekerProfileController(AethonDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Forbid();
        }

        var profile = await _dbContext.JobSeekerProfiles
            .AsNoTracking()
            .Include(x => x.Resumes)
                .ThenInclude(x => x.StoredFile)
            .FirstOrDefaultAsync(x => x.UserId == userId.Value, cancellationToken);

        if (profile is null)
        {
            return Ok(new JobSeekerProfileDto
            {
                UserId = userId.Value.ToString()
            });
        }

        return Ok(MapProfile(profile));
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        [FromBody] UpdateJobSeekerProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationErrors = ApiValidationHelper.Validate(request);
        if (validationErrors.Count > 0)
        {
            return ValidationProblemResult(validationErrors);
        }

        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Forbid();
        }

        var profile = await _dbContext.JobSeekerProfiles
            .Include(x => x.Resumes)
                .ThenInclude(x => x.StoredFile)
            .FirstOrDefaultAsync(x => x.UserId == userId.Value, cancellationToken);

        var utcNow = DateTime.UtcNow;

        if (profile is null)
        {
            profile = new JobSeekerProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                CreatedUtc = utcNow,
                CreatedByUserId = userId.Value
            };

            _dbContext.JobSeekerProfiles.Add(profile);
        }

        profile.FirstName = Clean(request.FirstName);
        profile.MiddleName = Clean(request.MiddleName);
        profile.LastName = Clean(request.LastName);
        profile.DateOfBirth = request.DateOfBirth;
        profile.PhoneNumber = Clean(request.PhoneNumber);
        profile.WhatsAppNumber = Clean(request.WhatsAppNumber);
        profile.Headline = Clean(request.Headline);
        profile.Summary = Clean(request.Summary);
        profile.AboutMe = Clean(request.AboutMe);
        profile.CurrentLocation = Clean(request.CurrentLocation);
        profile.PreferredLocation = Clean(request.PreferredLocation);
        profile.AvailabilityText = Clean(request.AvailabilityText);
        profile.LinkedInUrl = Clean(request.LinkedInUrl);
        profile.OpenToWork = request.OpenToWork;
        profile.DesiredSalaryFrom = request.DesiredSalaryFrom;
        profile.DesiredSalaryTo = request.DesiredSalaryTo;
        profile.DesiredSalaryCurrency = request.DesiredSalaryCurrency;
        profile.WillRelocate = request.WillRelocate;
        profile.RequiresSponsorship = request.RequiresSponsorship;
        profile.HasWorkRights = request.HasWorkRights;
        profile.IsPublicProfileEnabled = request.IsPublicProfileEnabled;
        profile.IsSearchable = request.IsSearchable;
        profile.Slug = Clean(request.Slug);
        profile.LastProfileUpdatedUtc = utcNow;
        profile.UpdatedUtc = utcNow;
        profile.UpdatedByUserId = userId.Value;

        await _dbContext.SaveChangesAsync(cancellationToken);

        profile = await _dbContext.JobSeekerProfiles
            .AsNoTracking()
            .Include(x => x.Resumes)
                .ThenInclude(x => x.StoredFile)
            .FirstAsync(x => x.UserId == userId.Value, cancellationToken);

        return Ok(MapProfile(profile));
    }

    [HttpPost("resumes")]
    public async Task<IActionResult> AddResume(
        [FromBody] AddJobSeekerResumeRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationErrors = ApiValidationHelper.Validate(request);
        if (validationErrors.Count > 0)
        {
            return ValidationProblemResult(validationErrors);
        }

        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Forbid();
        }

        var storedFile = await _dbContext.StoredFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.FileId && x.UploadedByUserId == userId.Value,
                cancellationToken);

        if (storedFile is null)
        {
            return NotFound();
        }

        var profile = await _dbContext.JobSeekerProfiles
            .Include(x => x.Resumes)
            .FirstOrDefaultAsync(x => x.UserId == userId.Value, cancellationToken);

        var utcNow = DateTime.UtcNow;

        if (profile is null)
        {
            profile = new JobSeekerProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                CreatedUtc = utcNow,
                CreatedByUserId = userId.Value
            };

            _dbContext.JobSeekerProfiles.Add(profile);
        }

        var existingResume = await _dbContext.JobSeekerResumes
            .AsNoTracking()
            .AnyAsync(
                x => x.StoredFileId == request.FileId &&
                     x.JobSeekerProfileId == profile.Id,
                cancellationToken);

        if (existingResume)
        {
            return Conflict(new
            {
                code = "jobseeker.resume.already_exists",
                message = "That file is already linked as a resume."
            });
        }

        if (request.IsDefault)
        {
            foreach (var resume in profile.Resumes.Where(x => x.IsDefault))
            {
                resume.IsDefault = false;
                resume.UpdatedUtc = utcNow;
                resume.UpdatedByUserId = userId.Value;
            }
        }

        var resumeEntity = new JobSeekerResume
        {
            Id = Guid.NewGuid(),
            JobSeekerProfileId = profile.Id,
            StoredFileId = storedFile.Id,
            Name = request.Name.Trim(),
            Description = Clean(request.Description),
            IsDefault = request.IsDefault || !profile.Resumes.Any(x => x.IsActive),
            IsActive = true,
            CreatedUtc = utcNow,
            CreatedByUserId = userId.Value
        };

        _dbContext.JobSeekerResumes.Add(resumeEntity);

        profile.LastProfileUpdatedUtc = utcNow;
        profile.UpdatedUtc = utcNow;
        profile.UpdatedByUserId = userId.Value;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdResume = await _dbContext.JobSeekerResumes
            .AsNoTracking()
            .Include(x => x.StoredFile)
            .FirstAsync(x => x.Id == resumeEntity.Id, cancellationToken);

        return Ok(MapResume(createdResume));
    }

    [HttpPost("resumes/{resumeId:guid}/default")]
    public async Task<IActionResult> SetDefaultResume(
        Guid resumeId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Forbid();
        }

        var profile = await _dbContext.JobSeekerProfiles
            .Include(x => x.Resumes)
            .FirstOrDefaultAsync(x => x.UserId == userId.Value, cancellationToken);

        if (profile is null)
        {
            return NotFound();
        }

        var resume = profile.Resumes.FirstOrDefault(x => x.Id == resumeId && x.IsActive);
        if (resume is null)
        {
            return NotFound();
        }

        var utcNow = DateTime.UtcNow;

        foreach (var item in profile.Resumes.Where(x => x.IsActive))
        {
            item.IsDefault = item.Id == resumeId;
            item.UpdatedUtc = utcNow;
            item.UpdatedByUserId = userId.Value;
        }

        profile.LastProfileUpdatedUtc = utcNow;
        profile.UpdatedUtc = utcNow;
        profile.UpdatedByUserId = userId.Value;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    [HttpDelete("resumes/{resumeId:guid}")]
    public async Task<IActionResult> RemoveResume(
        Guid resumeId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Forbid();
        }

        var profile = await _dbContext.JobSeekerProfiles
            .Include(x => x.Resumes)
            .FirstOrDefaultAsync(x => x.UserId == userId.Value, cancellationToken);

        if (profile is null)
        {
            return NotFound();
        }

        var resume = profile.Resumes.FirstOrDefault(x => x.Id == resumeId && x.IsActive);
        if (resume is null)
        {
            return NotFound();
        }

        var utcNow = DateTime.UtcNow;

        resume.IsActive = false;
        resume.IsDefault = false;
        resume.UpdatedUtc = utcNow;
        resume.UpdatedByUserId = userId.Value;

        var fallbackDefault = profile.Resumes
            .Where(x => x.Id != resumeId && x.IsActive)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.CreatedUtc)
            .FirstOrDefault();

        if (fallbackDefault is not null)
        {
            fallbackDefault.IsDefault = true;
            fallbackDefault.UpdatedUtc = utcNow;
            fallbackDefault.UpdatedByUserId = userId.Value;
        }

        profile.LastProfileUpdatedUtc = utcNow;
        profile.UpdatedUtc = utcNow;
        profile.UpdatedByUserId = userId.Value;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }

    private static JobSeekerProfileDto MapProfile(JobSeekerProfile profile)
    {
        return new JobSeekerProfileDto
        {
            UserId = profile.UserId.ToString(),
            FirstName = profile.FirstName,
            MiddleName = profile.MiddleName,
            LastName = profile.LastName,
            DateOfBirth = profile.DateOfBirth,
            PhoneNumber = profile.PhoneNumber,
            WhatsAppNumber = profile.WhatsAppNumber,
            Headline = profile.Headline,
            Summary = profile.Summary,
            AboutMe = profile.AboutMe,
            CurrentLocation = profile.CurrentLocation,
            PreferredLocation = profile.PreferredLocation,
            AvailabilityText = profile.AvailabilityText,
            LinkedInUrl = profile.LinkedInUrl,
            OpenToWork = profile.OpenToWork,
            DesiredSalaryFrom = profile.DesiredSalaryFrom,
            DesiredSalaryTo = profile.DesiredSalaryTo,
            DesiredSalaryCurrency = profile.DesiredSalaryCurrency,
            WillRelocate = profile.WillRelocate,
            RequiresSponsorship = profile.RequiresSponsorship,
            HasWorkRights = profile.HasWorkRights,
            IsPublicProfileEnabled = profile.IsPublicProfileEnabled,
            IsSearchable = profile.IsSearchable,
            Slug = profile.Slug,
            LastProfileUpdatedUtc = profile.LastProfileUpdatedUtc,
            Resumes = profile.Resumes
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.Name)
                .Select(MapResume)
                .ToList()
        };
    }

    private static JobSeekerResumeDto MapResume(JobSeekerResume resume)
    {
        return new JobSeekerResumeDto
        {
            Id = resume.Id,
            Name = resume.Name,
            Description = resume.Description,
            IsDefault = resume.IsDefault,
            IsActive = resume.IsActive,
            File = new StoredFileDto
            {
                Id = resume.StoredFile.Id,
                OriginalFileName = resume.StoredFile.OriginalFileName,
                ContentType = resume.StoredFile.ContentType,
                LengthBytes = resume.StoredFile.LengthBytes,
                DownloadUrl = $"/files/{resume.StoredFile.Id}/download"
            }
        };
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private ObjectResult ValidationProblemResult(Dictionary<string, string[]> errors)
    {
        foreach (var error in errors)
        {
            ModelState.AddModelError(error.Key, string.Join(" ", error.Value));
        }

        return (ObjectResult)ValidationProblem(ModelState);
    }
}
