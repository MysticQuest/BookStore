namespace BookStore.Application.Interfaces;

/// <summary>
/// Service interface for fetching books from an external API.
/// </summary>
public interface IBookFetchService
{
    /// <summary>
    /// Fetches books from the external API and saves any new books to the database.
    /// Only books with new 'Number' values are added (idempotent operation).
    /// </summary>
    /// <returns>The number of new books added.</returns>
    Task<int> FetchAndSaveBooksAsync(CancellationToken cancellationToken = default);
}
