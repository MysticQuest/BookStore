using System.Net.Http.Json;
using BookStore.Client.ViewModels;

namespace BookStore.Client.Services;

/// <summary>
/// HTTP client implementation for communicating with the Books API.
/// </summary>
public class BookApiClient : IBookApiClient
{
    private readonly HttpClient _httpClient;

    public BookApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<BookViewModel>> GetAllBooksAsync()
    {
        var books = await _httpClient.GetFromJsonAsync<IEnumerable<BookViewModel>>("api/books");
        return books ?? Enumerable.Empty<BookViewModel>();
    }

    public async Task<bool> UpdateCopiesAsync(Guid id, int numberOfCopies)
    {
        var request = new { NumberOfCopies = numberOfCopies };
        var response = await _httpClient.PutAsJsonAsync($"api/books/{id}/copies", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdatePriceAsync(Guid id, decimal price)
    {
        var request = new { Price = price };
        var response = await _httpClient.PutAsJsonAsync($"api/books/{id}/price", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<FetchResultViewModel> FetchBooksAsync()
    {
        var response = await _httpClient.PostAsync("api/books/fetch", null);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<FetchResultViewModel>();
            return result ?? new FetchResultViewModel { Message = "Fetch completed" };
        }

        return new FetchResultViewModel { Message = "Fetch failed", BooksAdded = 0 };
    }

    public async Task<bool> DeleteBookAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/books/{id}");
        return response.IsSuccessStatusCode;
    }
}
