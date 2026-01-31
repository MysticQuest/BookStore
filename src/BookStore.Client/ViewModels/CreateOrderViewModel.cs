using System.ComponentModel.DataAnnotations;

namespace BookStore.Client.ViewModels;

/// <summary>
/// View model for creating a new order.
/// </summary>
public class CreateOrderViewModel
{
    /// <summary>
    /// Client-generated ID for idempotent order creation.
    /// Generated when the form is opened, ensuring retries use the same ID.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
    public string Address { get; set; } = string.Empty;
}
