using System.Collections.Concurrent;
using BookStore.Application.Constants;
using BookStore.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BookStore.Infrastructure.Services;

/// <summary>
/// In-memory cache service implementation using IMemoryCache.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, bool> _cacheKeys;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
        _cacheKeys = new ConcurrentDictionary<string, bool>();
    }

    public T? Get<T>(string key)
    {
        return _cache.TryGetValue(key, out T? value) ? value : default;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
        };

        _cache.Set(key, value, options);
        _cacheKeys.TryAdd(key, true);
    }

    private void Remove(string key)
    {
        _cache.Remove(key);
        _cacheKeys.TryRemove(key, out _);
    }

    private void RemoveByPrefix(string prefix)
    {
        var keysToRemove = _cacheKeys.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            Remove(key);
        }
    }

    public void InvalidateBooksCache()
    {
        Remove(CacheKeys.AllBooks);
    }

    public void InvalidateOrdersCache()
    {
        RemoveByPrefix(CacheKeys.OrdersPrefix);
    }
}
