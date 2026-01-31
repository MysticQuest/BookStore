using System.ComponentModel.DataAnnotations;

namespace BookStore.Client.ViewModels;

/// <summary>
/// View model for creating a new order.
/// </summary>
public class CreateOrderViewModel
{
    [Required(ErrorMessage = "Address is required.")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
    public string Address { get; set; } = string.Empty;
}
