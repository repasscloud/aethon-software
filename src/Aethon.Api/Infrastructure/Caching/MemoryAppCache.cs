using System.Collections.Concurrent;
using Aethon.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace Aethon.Api.Infrastructure.Caching;

public sealed class MemoryAppCache : IAppCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    public MemoryAppCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue<T>(key, out var cached) && cached is not null)
        {
            return cached;
        }

        var value = await factory(cancellationToken);

        _memoryCache.Set(key, value, ttl);
        _keys.TryAdd(key, 0);

        return value;
    }

    public Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        _keys.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(
        string prefix,
        CancellationToken cancellationToken = default)
    {
        var matchingKeys = _keys.Keys
            .Where(x => x.StartsWith(prefix, StringComparison.Ordinal))
            .ToArray();

        foreach (var key in matchingKeys)
        {
            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
