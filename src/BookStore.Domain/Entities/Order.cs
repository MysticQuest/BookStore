namespace BookStore.Domain.Entities;

/// <summary>
/// Represents a customer order for books.
/// </summary>
public class Order
{
    /// <summary>
    /// Unique identifier for the order.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The shipping address for the order.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// The date and time when the order was created.
    /// </summary>
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// The calculated total cost from the selected books.
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// The books included in this order.
    /// </summary>
    public ICollection<OrderBook> OrderBooks { get; set; } = new List<OrderBook>();

    /// <summary>
    /// Concurrency token for optimistic locking.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];
}
