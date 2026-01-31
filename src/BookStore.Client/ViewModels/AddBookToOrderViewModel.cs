using System.ComponentModel.DataAnnotations;

namespace BookStore.Client.ViewModels;

/// <summary>
/// View model for adding a book to an order.
/// </summary>
public class AddBookToOrderViewModel
{
    [Required(ErrorMessage = "Please select a book.")]
    public Guid BookId { get; set; }

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10,000.")]
    public int Quantity { get; set; } = 1;
}
