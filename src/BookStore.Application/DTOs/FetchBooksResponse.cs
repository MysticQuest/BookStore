namespace BookStore.Application.DTOs;

/// <summary>
/// Response DTO for the book fetch operation.
/// </summary>
public record FetchBooksResponse
{
    /// <summary>
    /// A human-readable message describing the result.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// The number of new books added to the database.
    /// </summary>
    public int BooksAdded { get; init; }

    /// <summary>
    /// Creates a successful response for when books were added.
    /// </summary>
    public static FetchBooksResponse Success(int count) => new()
    {
        Message = count > 0
            ? $"Successfully added {count} new book(s)."
            : "No new books to add. All books already exist in the database.",
        BooksAdded = count
    };
}
