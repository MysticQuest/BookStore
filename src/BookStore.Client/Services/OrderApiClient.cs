using System.Net.Http.Json;
using System.Text.Json;
using BookStore.Client.ViewModels;

namespace BookStore.Client.Services;

/// <summary>
/// HTTP client implementation for communicating with the Orders API.
/// </summary>
public class OrderApiClient : IOrderApiClient
{
    private readonly HttpClient _httpClient;

    public OrderApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<OrderViewModel>> GetAllOrdersAsync()
    {
        var orders = await _httpClient.GetFromJsonAsync<IEnumerable<OrderViewModel>>("api/orders");
        return orders ?? Enumerable.Empty<OrderViewModel>();
    }

    public async Task<OrderViewModel?> CreateOrderAsync(CreateOrderViewModel model)
    {
        var request = new { Id = model.Id, Address = model.Address };
        var response = await _httpClient.PostAsJsonAsync("api/orders", request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<OrderViewModel>();
        }

        return null;
    }

    public async Task<IEnumerable<OrderBookViewModel>> GetOrderBooksAsync(Guid orderId)
    {
        var books = await _httpClient.GetFromJsonAsync<IEnumerable<OrderBookViewModel>>($"api/orders/{orderId}/books");
        return books ?? Enumerable.Empty<OrderBookViewModel>();
    }

    public async Task<(bool Success, string? ErrorMessage)> AddBookToOrderAsync(Guid orderId, AddBookToOrderViewModel model)
    {
        var request = new { BookId = model.BookId, Quantity = model.Quantity };
        var response = await _httpClient.PostAsJsonAsync($"api/orders/{orderId}/books", request);

        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        try
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
            if (errorObj.TryGetProperty("message", out var messageElement))
            {
                return (false, messageElement.GetString());
            }
        }
        catch
        {
            // Ignore JSON parsing errors
        }

        return (false, $"Failed to add book to order. Status: {response.StatusCode}");
    }

    public async Task<bool> RemoveBookFromOrderAsync(Guid orderId, Guid bookId)
    {
        var response = await _httpClient.DeleteAsync($"api/orders/{orderId}/books/{bookId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteOrderAsync(Guid orderId)
    {
        var response = await _httpClient.DeleteAsync($"api/orders/{orderId}");
        return response.IsSuccessStatusCode;
    }
}
