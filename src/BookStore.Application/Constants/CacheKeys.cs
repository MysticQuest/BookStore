namespace BookStore.Application.Constants;

/// <summary>
/// Centralized cache key constants used throughout the application.
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// Cache key for all books.
    /// </summary>
    public const string AllBooks = "books:all";

    /// <summary>
    /// Cache key for all orders.
    /// </summary>
    public const string AllOrders = "orders:all";

    /// <summary>
    /// Prefix for order-related cache keys.
    /// </summary>
    public const string OrdersPrefix = "orders:";

    /// <summary>
    /// Gets the cache key for a specific order's books.
    /// </summary>
    public static string OrderBooks(Guid orderId) => $"orders:{orderId}:books";
}
