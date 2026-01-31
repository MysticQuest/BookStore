using BookStore.Client.ViewModels;

namespace BookStore.Client.Services;

/// <summary>
/// Client interface for communicating with the Books API.
/// </summary>
public interface IBookApiClient
{
    /// <summary>
    /// Gets all books from the API.
    /// </summary>
    Task<IEnumerable<BookViewModel>> GetAllBooksAsync();

    /// <summary>
    /// Updates the number of copies for a specific book.
    /// </summary>
    Task<bool> UpdateCopiesAsync(Guid id, int numberOfCopies);

    /// <summary>
    /// Updates the price for a specific book.
    /// </summary>
    Task<bool> UpdatePriceAsync(Guid id, decimal price);

    /// <summary>
    /// Triggers the fetch of books from the external API.
    /// </summary>
    Task<FetchResultViewModel> FetchBooksAsync();

    /// <summary>
    /// Deletes a specific book.
    /// </summary>
    Task<bool> DeleteBookAsync(Guid id);
}
