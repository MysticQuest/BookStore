namespace BookStore.Application.DTOs;

/// <summary>
/// Data transfer object for Book entity.
/// </summary>
public class BookDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalTitle { get; set; } = string.Empty;
    public DateTime? ReleaseDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Pages { get; set; }
    public string Cover { get; set; } = string.Empty;
    public int Index { get; set; }
    public int NumberOfCopies { get; set; }
    public decimal Price { get; set; }
}
