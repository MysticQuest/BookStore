namespace BookStore.Application.Interfaces;

/// <summary>
/// Service for managing application cache.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by key.
    /// </summary>
    T? Get<T>(string key);

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    void Set<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Invalidates all books-related cache entries.
    /// </summary>
    void InvalidateBooksCache();

    /// <summary>
    /// Invalidates all orders-related cache entries.
    /// </summary>
    void InvalidateOrdersCache();
}
