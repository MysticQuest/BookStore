namespace BookStore.Client.ViewModels;

/// <summary>
/// View model for displaying order information.
/// </summary>
public class OrderViewModel
{
    public Guid Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Formatted creation date for display.
    /// </summary>
    public string DisplayCreationDate => CreationDate.ToString("MMM d, yyyy HH:mm");

    /// <summary>
    /// Formatted total cost with Euro symbol.
    /// </summary>
    public string DisplayTotalCost => TotalCost.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));

    /// <summary>
    /// Truncated address for table display.
    /// </summary>
    public string DisplayAddress => Address.Length > 50 ? Address[..47] + "..." : Address;
}
