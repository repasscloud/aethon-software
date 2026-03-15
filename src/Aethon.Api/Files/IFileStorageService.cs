namespace Aethon.Api.Files;

public interface IFileStorageService
{
    Task<string> SaveAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<(Stream Stream, string ContentType, string OriginalFileName)?> OpenReadAsync(
        string storagePath,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default);
}
