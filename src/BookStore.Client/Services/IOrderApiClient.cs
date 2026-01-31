using BookStore.Client.ViewModels;

namespace BookStore.Client.Services;

/// <summary>
/// Interface for communicating with the Orders API.
/// </summary>
public interface IOrderApiClient
{
    /// <summary>
    /// Gets all orders.
    /// </summary>
    Task<IEnumerable<OrderViewModel>> GetAllOrdersAsync();

    /// <summary>
    /// Creates a new order.
    /// </summary>
    Task<OrderViewModel?> CreateOrderAsync(CreateOrderViewModel model);

    /// <summary>
    /// Gets the books for a specific order.
    /// </summary>
    Task<IEnumerable<OrderBookViewModel>> GetOrderBooksAsync(Guid orderId);

    /// <summary>
    /// Adds a book to an order.
    /// </summary>
    /// <returns>Tuple with success flag and error message if failed.</returns>
    Task<(bool Success, string? ErrorMessage)> AddBookToOrderAsync(Guid orderId, AddBookToOrderViewModel model);

    /// <summary>
    /// Removes a book from an order.
    /// </summary>
    Task<bool> RemoveBookFromOrderAsync(Guid orderId, Guid bookId);

    /// <summary>
    /// Deletes an order.
    /// </summary>
    Task<bool> DeleteOrderAsync(Guid orderId);
}
