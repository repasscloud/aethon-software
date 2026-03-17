using System.Security.Claims;
using Aethon.Api.Files;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Controllers;

[ApiController]
[Authorize]
[Route("files")]
public sealed class FilesController : ControllerBase
{
    private static readonly HashSet<string> AllowedResumeContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    private readonly AethonDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;

    public FilesController(AethonDbContext dbContext, IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
    }

    [HttpPost("resume")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadResume(IFormFile? file, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Forbid();
        }

        if (file is null || file.Length == 0)
        {
            return ValidationProblemResult(new Dictionary<string, string[]>
            {
                ["File"] = ["A file is required."]
            });
        }

        if (!AllowedResumeContentTypes.Contains(file.ContentType))
        {
            return ValidationProblemResult(new Dictionary<string, string[]>
            {
                ["File"] = ["Only PDF, DOC, and DOCX files are allowed."]
            });
        }

        var storagePath = await _fileStorageService.SaveAsync(
            file.OpenReadStream(),
            file.FileName,
            cancellationToken);

        var storedFile = new StoredFile
        {
            Id = NewGuidId(),
            FileName = Path.GetFileName(storagePath),
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            LengthBytes = file.Length,
            StorageProvider = "local",
            StoragePath = storagePath,
            UploadedByUserId = userId,
            CreatedUtc = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _dbContext.StoredFiles.Add(storedFile);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new UploadFileResultDto
        {
            Id = storedFile.Id,
            OriginalFileName = storedFile.OriginalFileName,
            ContentType = storedFile.ContentType,
            LengthBytes = storedFile.LengthBytes,
            DownloadUrl = $"/files/{storedFile.Id}/download"
        });
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var file = await _dbContext.StoredFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (file is null)
        {
            return NotFound();
        }

        var currentUserIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(currentUserIdValue, out var currentUserId);

        var isOwner = file.UploadedByUserId == currentUserId;

        var isLinkedToApplication = await _dbContext.JobApplications
            .AsNoTracking()
            .AnyAsync(x =>
                x.ResumeFileId == id &&
                x.Job.OwnedByOrganisation.Memberships.Any(m =>
                    m.UserId == currentUserId &&
                    m.Status == Shared.Enums.MembershipStatus.Active),
                cancellationToken);

        if (!isOwner && !isLinkedToApplication)
        {
            return Forbid();
        }

        var opened = await _fileStorageService.OpenReadAsync(
            file.StoragePath,
            file.OriginalFileName,
            file.ContentType,
            cancellationToken);

        if (opened is null)
        {
            return NotFound();
        }

        return File(opened.Value.Stream, opened.Value.ContentType, opened.Value.OriginalFileName);
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
