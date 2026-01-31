namespace BookStore.Application.DTOs;

/// <summary>
/// Data transfer object for a book within an order.
/// </summary>
public class OrderBookDto
{
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
    public decimal Subtotal => Quantity * PriceAtPurchase;
}
