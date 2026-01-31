using BookStore.Api.Controllers;
using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookStore.Api.Tests.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<OrdersController>> _loggerMock;
    private readonly OrdersController _sut;

    public OrdersControllerTests()
    {
        _orderServiceMock = new Mock<IOrderService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<OrdersController>>();

        _sut = new OrdersController(
            _orderServiceMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllOrders_ReturnsOk_WithOrdersFromCache()
    {
        // Arrange
        var cachedOrders = new List<OrderDto>
        {
            new() { Id = Guid.NewGuid(), Address = "Cached Address" }
        };
        _cacheServiceMock.Setup(c => c.Get<IEnumerable<OrderDto>>(It.IsAny<string>()))
            .Returns(cachedOrders);

        // Act
        var result = await _sut.GetAllOrders(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().Be(cachedOrders);
        _orderServiceMock.Verify(s => s.GetAllOrdersAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllOrders_ReturnsOk_WithOrdersFromDatabase_WhenNotCached()
    {
        // Arrange
        _cacheServiceMock.Setup(c => c.Get<IEnumerable<OrderDto>>(It.IsAny<string>()))
            .Returns((IEnumerable<OrderDto>?)null);

        var orders = new List<OrderDto>
        {
            new() { Id = Guid.NewGuid(), Address = "Order 1" },
            new() { Id = Guid.NewGuid(), Address = "Order 2" }
        };
        _orderServiceMock.Setup(s => s.GetAllOrdersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _sut.GetAllOrders(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().Be(orders);
        _cacheServiceMock.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<IEnumerable<OrderDto>>(), null), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_ReturnsCreated_WithNewOrder()
    {
        // Arrange
        var request = new CreateOrderRequest { Address = "Test Address" };
        var order = new OrderDto
        {
            Id = Guid.NewGuid(),
            Address = "Test Address",
            CreationDate = DateTime.UtcNow,
            TotalCost = 0
        };
        _orderServiceMock.Setup(s => s.CreateOrderAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _sut.CreateOrder(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)result.Result!;
        createdResult.Value.Should().Be(order);
        _cacheServiceMock.Verify(c => c.InvalidateOrdersCache(), Times.Once);
    }

    [Fact]
    public async Task GetOrderBooks_ReturnsOk_WithBooksFromCache()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var cachedBooks = new List<OrderBookDto>
        {
            new() { BookId = Guid.NewGuid(), BookTitle = "Cached Book", Quantity = 2 }
        };
        _cacheServiceMock.Setup(c => c.Get<IEnumerable<OrderBookDto>>(It.IsAny<string>()))
            .Returns(cachedBooks);

        // Act
        var result = await _sut.GetOrderBooks(orderId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().Be(cachedBooks);
    }

    [Fact]
    public async Task GetOrderBooks_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _cacheServiceMock.Setup(c => c.Get<IEnumerable<OrderBookDto>>(It.IsAny<string>()))
            .Returns((IEnumerable<OrderBookDto>?)null);
        _orderServiceMock.Setup(s => s.GetOrderByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var result = await _sut.GetOrderBooks(orderId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AddBookToOrder_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new AddBookToOrderRequest { BookId = Guid.NewGuid(), Quantity = 1 };
        _orderServiceMock.Setup(s => s.AddBookToOrderAsync(orderId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, null));

        // Act
        var result = await _sut.AddBookToOrder(orderId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _cacheServiceMock.Verify(c => c.InvalidateBooksCache(), Times.Once);
        _cacheServiceMock.Verify(c => c.InvalidateOrdersCache(), Times.Once);
    }

    [Fact]
    public async Task AddBookToOrder_ReturnsNotFound_WhenOrderOrBookNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new AddBookToOrderRequest { BookId = Guid.NewGuid(), Quantity = 1 };
        _orderServiceMock.Setup(s => s.AddBookToOrderAsync(orderId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "Order not found"));

        // Act
        var result = await _sut.AddBookToOrder(orderId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AddBookToOrder_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new AddBookToOrderRequest { BookId = Guid.NewGuid(), Quantity = 100 };
        _orderServiceMock.Setup(s => s.AddBookToOrderAsync(orderId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "Not enough copies available"));

        // Act
        var result = await _sut.AddBookToOrder(orderId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RemoveBookFromOrder_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        _orderServiceMock.Setup(s => s.RemoveBookFromOrderAsync(orderId, bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.RemoveBookFromOrder(orderId, bookId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _cacheServiceMock.Verify(c => c.InvalidateBooksCache(), Times.Once);
        _cacheServiceMock.Verify(c => c.InvalidateOrdersCache(), Times.Once);
    }

    [Fact]
    public async Task RemoveBookFromOrder_ReturnsNotFound_WhenNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        _orderServiceMock.Setup(s => s.RemoveBookFromOrderAsync(orderId, bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.RemoveBookFromOrder(orderId, bookId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteOrder_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderServiceMock.Setup(s => s.DeleteOrderAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteOrder(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _cacheServiceMock.Verify(c => c.InvalidateBooksCache(), Times.Once);
        _cacheServiceMock.Verify(c => c.InvalidateOrdersCache(), Times.Once);
    }

    [Fact]
    public async Task DeleteOrder_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderServiceMock.Setup(s => s.DeleteOrderAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteOrder(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
