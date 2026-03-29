using Aethon.Application.Abstractions.Files;

namespace Aethon.Api.Infrastructure.Files;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveAsync(
        string fileName,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        var uploadsRoot = GetUploadsRoot();
        Directory.CreateDirectory(uploadsRoot);

        var fullPath = Path.Combine(uploadsRoot, fileName);

        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);

        return fileName;
    }

    public Task<Stream> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        var uploadsRoot = GetUploadsRoot();
        var fullPath = Path.Combine(uploadsRoot, storagePath);

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    private string GetUploadsRoot()
    {
        return Path.Combine(_environment.ContentRootPath, "uploads");
    }
}
