namespace BookStore.Client.ViewModels;

/// <summary>
/// View model for displaying a book within an order.
/// </summary>
public class OrderBookViewModel
{
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
    public decimal Subtotal => Quantity * PriceAtPurchase;

    /// <summary>
    /// Formatted price with Euro symbol.
    /// </summary>
    public string DisplayPrice => PriceAtPurchase.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));

    /// <summary>
    /// Formatted subtotal with Euro symbol.
    /// </summary>
    public string DisplaySubtotal => Subtotal.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
}
