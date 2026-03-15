namespace Aethon.Api.Files;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> SaveAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var root = _configuration["Files:RootPath"] ?? "/data/files";
        Directory.CreateDirectory(root);

        var ext = Path.GetExtension(fileName);
        var safeName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(root, safeName);

        await using var fileStream = File.Create(fullPath);
        await stream.CopyToAsync(fileStream, cancellationToken);

        return fullPath;
    }

    public Task<(Stream Stream, string ContentType, string OriginalFileName)?> OpenReadAsync(
        string storagePath,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(storagePath))
        {
            return Task.FromResult<(Stream, string, string)?>(null);
        }

        Stream stream = File.OpenRead(storagePath);
        return Task.FromResult<(Stream, string, string)?>((stream, contentType, originalFileName));
    }
}
