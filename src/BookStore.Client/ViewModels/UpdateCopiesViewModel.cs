using System.ComponentModel.DataAnnotations;

namespace BookStore.Client.ViewModels;

/// <summary>
/// View model for updating the number of copies of a book.
/// </summary>
public class UpdateCopiesViewModel
{
    [Required(ErrorMessage = "Number of copies is required.")]
    [Range(0, 100000, ErrorMessage = "Number of copies must be between 0 and 100,000.")]
    public int NumberOfCopies { get; set; }
}
