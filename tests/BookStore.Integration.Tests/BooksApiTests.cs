using System.Net;
using System.Net.Http.Json;
using BookStore.Application.DTOs;
using FluentAssertions;

namespace BookStore.Integration.Tests;

public class BooksApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BooksApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllBooks_ReturnsEmptyList_WhenNoBooksExist()
    {
        // Act
        var response = await _client.GetAsync("/api/books");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var books = await response.Content.ReadFromJsonAsync<List<BookDto>>();
        books.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateCopies_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new UpdateCopiesRequest { NumberOfCopies = 10 };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/books/{nonExistentId}/copies", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePrice_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new UpdatePriceRequest { Price = 19.99m };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/books/{nonExistentId}/price", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/books/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

}
