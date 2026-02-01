using System.ComponentModel.DataAnnotations;

namespace BookStore.Application.DTOs;

/// <summary>
/// Request DTO for updating the number of copies of a book.
/// </summary>
public class UpdateCopiesRequest
{
    [Required]
    [Range(0, 100000, ErrorMessage = "Number of copies must be between 0 and 100,000.")]
    public int NumberOfCopies { get; set; }
}
