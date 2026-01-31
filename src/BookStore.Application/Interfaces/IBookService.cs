using BookStore.Application.DTOs;

namespace BookStore.Application.Interfaces;

/// <summary>
/// Service interface for book management operations.
/// </summary>
public interface IBookService
{
    /// <summary>
    /// Gets all books.
    /// </summary>
    Task<IEnumerable<BookDto>> GetAllBooksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a book by its unique identifier.
    /// </summary>
    Task<BookDto?> GetBookByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the number of copies for a specific book.
    /// </summary>
    Task<bool> UpdateNumberOfCopiesAsync(Guid id, int numberOfCopies, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the price for a specific book.
    /// </summary>
    Task<bool> UpdatePriceAsync(Guid id, decimal price, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all books from the database. For debugging purposes only.
    /// </summary>
    Task DeleteAllBooksAsync(CancellationToken cancellationToken = default);
}
