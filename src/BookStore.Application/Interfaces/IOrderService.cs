using BookStore.Application.DTOs;

namespace BookStore.Application.Interfaces;

/// <summary>
/// Service interface for order management operations.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Gets all orders (summary only).
    /// </summary>
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an order by its unique identifier.
    /// </summary>
    Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new empty order.
    /// </summary>
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the books for a specific order.
    /// </summary>
    Task<IEnumerable<OrderBookDto>> GetOrderBooksAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a book to an order.
    /// </summary>
    /// <returns>A result indicating success or failure with an error message.</returns>
    Task<(bool Success, string? ErrorMessage)> AddBookToOrderAsync(Guid orderId, AddBookToOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a book from an order.
    /// </summary>
    Task<bool> RemoveBookFromOrderAsync(Guid orderId, Guid bookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an order by its unique identifier.
    /// </summary>
    Task<bool> DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default);
}
