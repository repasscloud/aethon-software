using Aethon.Application.Abstractions.Authentication;
using Aethon.Application.Abstractions.Files;
using Aethon.Application.Abstractions.Time;
using Aethon.Application.Common.Results;
using Aethon.Data;
using Aethon.Data.Entities;
using Aethon.Shared.Files;

namespace Aethon.Application.Files.Commands.UploadStoredFile;

public sealed class UploadStoredFileHandler
{
    private readonly AethonDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IFileStorageService _fileStorageService;

    public UploadStoredFileHandler(
        AethonDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IDateTimeProvider dateTimeProvider,
        IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _dateTimeProvider = dateTimeProvider;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<UploadFileResultDto>> HandleAsync(
        UploadStoredFileCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || _currentUserAccessor.UserId == Guid.Empty)
        {
            return Result<UploadFileResultDto>.Failure(
                "auth.unauthenticated",
                "The current user is not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(command.OriginalFileName))
        {
            return Result<UploadFileResultDto>.Failure(
                "files.name_required",
                "A file name is required.");
        }

        if (command.Content.Length == 0)
        {
            return Result<UploadFileResultDto>.Failure(
                "files.empty",
                "The uploaded file is empty.");
        }

        var safeOriginalFileName = Path.GetFileName(command.OriginalFileName.Trim());
        var extension = Path.GetExtension(safeOriginalFileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var storagePath = await _fileStorageService.SaveAsync(
            storedFileName,
            command.Content,
            cancellationToken);

        var utcNow = _dateTimeProvider.UtcNow;
        var storedFile = new StoredFile
        {
            Id = Guid.NewGuid(),
            FileName = storedFileName,
            OriginalFileName = safeOriginalFileName,
            ContentType = string.IsNullOrWhiteSpace(command.ContentType)
                ? "application/octet-stream"
                : command.ContentType.Trim(),
            LengthBytes = command.Content.LongLength,
            StorageProvider = "Local",
            StoragePath = storagePath,
            UploadedByUserId = _currentUserAccessor.UserId,
            CreatedUtc = utcNow,
            CreatedByUserId = _currentUserAccessor.UserId
        };

        _dbContext.StoredFiles.Add(storedFile);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<UploadFileResultDto>.Success(new UploadFileResultDto
        {
            Id = storedFile.Id,
            OriginalFileName = storedFile.OriginalFileName,
            ContentType = storedFile.ContentType,
            LengthBytes = storedFile.LengthBytes,
            DownloadUrl = $"/files/{storedFile.Id}/download"
        });
    }
}
