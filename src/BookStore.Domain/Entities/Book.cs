namespace BookStore.Domain.Entities;

/// <summary>
/// Represents a book in the store inventory.
/// </summary>
public class Book
{
    /// <summary>
    /// Unique identifier for the book.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Book number from the external API. Used for matching/idempotency.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// The title of the book.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The original title of the book.
    /// </summary>
    public string OriginalTitle { get; set; } = string.Empty;

    /// <summary>
    /// The release date of the book.
    /// </summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    /// A description of the book's content.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The number of pages in the book.
    /// </summary>
    public int Pages { get; set; }

    /// <summary>
    /// URL to the book's cover image.
    /// </summary>
    public string Cover { get; set; } = string.Empty;

    /// <summary>
    /// Index position from the external API.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// The available number of copies in inventory.
    /// </summary>
    public int NumberOfCopies { get; set; }

    /// <summary>
    /// The price of the book.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// The orders that include this book.
    /// </summary>
    public ICollection<OrderBook> OrderBooks { get; set; } = new List<OrderBook>();
}
