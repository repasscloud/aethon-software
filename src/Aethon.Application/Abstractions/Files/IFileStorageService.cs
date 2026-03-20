namespace Aethon.Application.Abstractions.Files;

public interface IFileStorageService
{
    Task<string> SaveAsync(
        string fileName,
        byte[] content,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default);
}
