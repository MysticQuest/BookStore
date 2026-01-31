using System.Net;
using System.Net.Http.Json;
using BookStore.Application.DTOs;
using FluentAssertions;

namespace BookStore.Integration.Tests;

public class OrdersApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdersApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllOrders_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
        orders.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateOrder_ReturnsCreated_WithValidAddress()
    {
        // Arrange
        var request = new CreateOrderRequest { Address = "123 Test Street" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.Address.Should().Be("123 Test Street");
        order.TotalCost.Should().Be(0);
        order.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateOrder_ReturnsBadRequest_WithEmptyAddress()
    {
        // Arrange
        var request = new CreateOrderRequest { Address = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrderBooks_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/orders/{nonExistentId}/books");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddBookToOrder_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();
        var request = new AddBookToOrderRequest { BookId = Guid.NewGuid(), Quantity = 1 };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/orders/{nonExistentOrderId}/books", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteOrder_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/orders/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

}
