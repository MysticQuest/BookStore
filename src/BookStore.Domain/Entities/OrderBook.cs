namespace BookStore.Domain.Entities;

/// <summary>
/// Represents a book included in an order with its quantity.
/// This is a junction entity for the many-to-many relationship between Order and Book.
/// </summary>
public class OrderBook
{
    /// <summary>
    /// The order this book belongs to.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// The book that was ordered.
    /// </summary>
    public Guid BookId { get; set; }

    /// <summary>
    /// The number of copies of this book in the order.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// The price of the book at the time of purchase.
    /// This captures the historical price in case the book price changes later.
    /// </summary>
    public decimal PriceAtPurchase { get; set; }

    /// <summary>
    /// Navigation property to the Order.
    /// </summary>
    public Order Order { get; set; } = null!;

    /// <summary>
    /// Navigation property to the Book.
    /// </summary>
    public Book Book { get; set; } = null!;
}
