using System.ComponentModel.DataAnnotations;

namespace BookStore.Application.DTOs;

/// <summary>
/// Request DTO for creating a new order.
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// Optional client-generated ID for idempotent order creation.
    /// If provided and an order with this ID exists, the existing order is returned.
    /// If not provided, the server generates a new ID.
    /// </summary>
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
    public string Address { get; set; } = string.Empty;
}
