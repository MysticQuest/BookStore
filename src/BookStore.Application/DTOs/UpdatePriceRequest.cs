using System.ComponentModel.DataAnnotations;

namespace BookStore.Application.DTOs;

/// <summary>
/// Request DTO for updating the price of a book.
/// </summary>
public class UpdatePriceRequest
{
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative value.")]
    public decimal Price { get; set; }
}
