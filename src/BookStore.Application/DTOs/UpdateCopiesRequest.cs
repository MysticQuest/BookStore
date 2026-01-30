using System.ComponentModel.DataAnnotations;

namespace BookStore.Application.DTOs;

/// <summary>
/// Request DTO for updating the number of copies of a book.
/// </summary>
public class UpdateCopiesRequest
{
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Number of copies must be a non-negative value.")]
    public int NumberOfCopies { get; set; }
}
