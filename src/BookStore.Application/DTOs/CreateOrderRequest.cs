using System.ComponentModel.DataAnnotations;

namespace BookStore.Application.DTOs;

/// <summary>
/// Request DTO for creating a new order.
/// </summary>
public class CreateOrderRequest
{
    [Required(ErrorMessage = "Address is required.")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
    public string Address { get; set; } = string.Empty;
}
