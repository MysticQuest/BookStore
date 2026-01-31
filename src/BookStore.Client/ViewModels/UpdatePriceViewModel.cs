using System.ComponentModel.DataAnnotations;

namespace BookStore.Client.ViewModels;

/// <summary>
/// View model for updating the price of a book.
/// </summary>
public class UpdatePriceViewModel
{
    [Required(ErrorMessage = "Price is required.")]
    [Range(0, 9999.99, ErrorMessage = "Price must be between €0.00 and €9,999.99.")]
    [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Price must have at most 2 decimal places.")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }
}
