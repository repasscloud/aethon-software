using System.Security.Claims;
using Aethon.Api.Infrastructure;
using Aethon.Data;
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
    public async Task<IActionResult> Get()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Forbid();
        }

        var profile = await _dbContext.JobSeekerProfiles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new
            {
                Profile = x,
                Resume = x.ResumeFileId == null
                    ? null
                    : _dbContext.StoredFiles
                        .Where(f => f.Id == x.ResumeFileId)
                        .Select(f => new StoredFileDto
                        {
                            Id = f.Id,
                            OriginalFileName = f.OriginalFileName,
                            ContentType = f.ContentType,
                            LengthBytes = f.LengthBytes,
                            DownloadUrl = $"/files/{f.Id}/download"
                        })
                        .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (profile is null)
        {
            return NotFound();
        }

        return Ok(new JobSeekerProfileDto
        {
            UserId = userId.ToString(),
            Headline = profile.Profile.Headline,
            Summary = profile.Profile.Summary,
            CurrentLocation = profile.Profile.CurrentLocation,
            PreferredLocation = profile.Profile.PreferredLocation,
            LinkedInUrl = profile.Profile.LinkedInUrl,
            OpenToWork = profile.Profile.OpenToWork,
            DesiredSalaryFrom = profile.Profile.DesiredSalaryFrom,
            DesiredSalaryTo = profile.Profile.DesiredSalaryTo,
            DesiredSalaryCurrency = profile.Profile.DesiredSalaryCurrency,
            ResumeFile = profile.Resume
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateJobSeekerProfileRequestDto request)
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

        var profile = await _dbContext.JobSeekerProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile is null)
        {
            return NotFound();
        }

        profile.Headline = Clean(request.Headline);
        profile.Summary = Clean(request.Summary);
        profile.CurrentLocation = Clean(request.CurrentLocation);
        profile.PreferredLocation = Clean(request.PreferredLocation);
        profile.LinkedInUrl = Clean(request.LinkedInUrl);
        profile.OpenToWork = request.OpenToWork;
        profile.DesiredSalaryFrom = request.DesiredSalaryFrom;
        profile.DesiredSalaryTo = request.DesiredSalaryTo;
        profile.DesiredSalaryCurrency = request.DesiredSalaryCurrency;
        profile.UpdatedUtc = DateTime.UtcNow;
        profile.UpdatedByUserId = userId.ToString();

        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("resume/{fileId}")]
    public async Task<IActionResult> SetResume(string fileId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Forbid();
        }

        var file = await _dbContext.StoredFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == fileId && x.UploadedByUserId == userId);

        if (file is null)
        {
            return NotFound();
        }

        var profile = await _dbContext.JobSeekerProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile is null)
        {
            return NotFound();
        }

        profile.ResumeFileId = fileId;
        profile.UpdatedUtc = DateTime.UtcNow;
        profile.UpdatedByUserId = userId.ToString();

        await _dbContext.SaveChangesAsync();

        return Ok();
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
