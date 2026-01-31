using System.ComponentModel.DataAnnotations;

namespace BookStore.Application.DTOs;

/// <summary>
/// Request DTO for updating the price of a book.
/// </summary>
public class UpdatePriceRequest
{
    [Required]
    [Range(0, 9999.99, ErrorMessage = "Price must be between 0 and 9999.99.")]
    [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Price can have at most 2 decimal places.")]
    public decimal Price { get; set; }
}
