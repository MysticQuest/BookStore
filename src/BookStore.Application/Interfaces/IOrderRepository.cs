using BookStore.Domain.Entities;

namespace BookStore.Application.Interfaces;

/// <summary>
/// Repository interface for Order entity operations.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Gets all orders from the database.
    /// </summary>
    Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an order by its unique identifier.
    /// </summary>
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an order with its books by order ID.
    /// </summary>
    Task<Order?> GetByIdWithBooksAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new order to the database.
    /// </summary>
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific order book entry.
    /// </summary>
    Task<OrderBook?> GetOrderBookAsync(Guid orderId, Guid bookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a book to an order.
    /// </summary>
    Task AddOrderBookAsync(OrderBook orderBook, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a book from an order.
    /// </summary>
    void RemoveOrderBook(OrderBook orderBook);

    /// <summary>
    /// Deletes an order by its unique identifier.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a book from all orders and updates order totals.
    /// </summary>
    Task RemoveBookFromAllOrdersAsync(Guid bookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
