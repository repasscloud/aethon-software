using Aethon.Api.Common;
using Aethon.Application.Abstractions.Files;
using Aethon.Application.Files.Commands.UploadStoredFile;
using Aethon.Data;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Endpoints.Files;

public static class FileEndpoints
{
    public static void MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/files")
            .RequireAuthorization()
            .WithTags("Files");

        group.MapPost(string.Empty, async (
            HttpContext httpContext,
            UploadStoredFileHandler handler,
            IFormFile file,
            CancellationToken ct) =>
        {
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest(new ApiError
                {
                    Code = "files.empty",
                    Message = "A non-empty file is required."
                });
            }

            await using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, ct);

            var command = new UploadStoredFileCommand
            {
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                Content = memoryStream.ToArray()
            };

            var validation = await httpContext.ValidateAsync(command, ct);
            if (validation is not null)
            {
                return validation;
            }

            var result = await handler.HandleAsync(command, ct);
            return result.ToMinimalApiResult();
        })
        .DisableAntiforgery();

        group.MapGet("/{fileId:guid}/download", async (
            AethonDbContext dbContext,
            IFileStorageService fileStorageService,
            Guid fileId,
            CancellationToken ct) =>
        {
            var storedFile = await dbContext.StoredFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == fileId, ct);

            if (storedFile is null)
            {
                return Results.NotFound(new ApiError
                {
                    Code = "files.not_found",
                    Message = "The requested file was not found."
                });
            }

            var stream = await fileStorageService.OpenReadAsync(storedFile.StoragePath, ct);

            return Results.File(
                stream,
                storedFile.ContentType,
                storedFile.OriginalFileName);
        });
    }
}
