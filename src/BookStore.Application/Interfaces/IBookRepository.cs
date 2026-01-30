using BookStore.Domain.Entities;

namespace BookStore.Application.Interfaces;

/// <summary>
/// Repository interface for Book entity operations.
/// </summary>
public interface IBookRepository
{
    /// <summary>
    /// Gets all books from the database.
    /// </summary>
    Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a book by its unique identifier.
    /// </summary>
    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a book by its number (from external API).
    /// </summary>
    Task<Book?> GetByNumberAsync(int number, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all existing book numbers from the database.
    /// </summary>
    Task<IEnumerable<int>> GetExistingBookNumbersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple books to the database.
    /// </summary>
    Task AddRangeAsync(IEnumerable<Book> books, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the price of a specific book.
    /// </summary>
    /// <returns>True if the book was found and updated, false otherwise.</returns>
    Task<bool> UpdatePriceAsync(Guid id, decimal price, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the number of copies of a specific book.
    /// </summary>
    /// <returns>True if the book was found and updated, false otherwise.</returns>
    Task<bool> UpdateNumberOfCopiesAsync(Guid id, int numberOfCopies, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
