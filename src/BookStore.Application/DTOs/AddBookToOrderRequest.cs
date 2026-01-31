using System.ComponentModel.DataAnnotations;

namespace BookStore.Application.DTOs;

/// <summary>
/// Request DTO for adding a book to an order.
/// </summary>
public class AddBookToOrderRequest
{
    [Required(ErrorMessage = "BookId is required.")]
    public Guid BookId { get; set; }

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10,000.")]
    public int Quantity { get; set; }
}
