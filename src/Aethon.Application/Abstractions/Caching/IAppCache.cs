namespace Aethon.Application.Abstractions.Caching;

public interface IAppCache
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task RemoveByPrefixAsync(
        string prefix,
        CancellationToken cancellationToken = default);
}
