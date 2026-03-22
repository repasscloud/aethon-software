namespace Aethon.Application.Abstractions.Syndication;

public interface IGoogleIndexingService
{
    Task NotifyPublishedAsync(Guid jobId, string jobUrl, CancellationToken ct = default);
    Task NotifyUpdatedAsync(Guid jobId, string jobUrl, CancellationToken ct = default);
    Task NotifyRemovedAsync(Guid jobId, string jobUrl, CancellationToken ct = default);
}
