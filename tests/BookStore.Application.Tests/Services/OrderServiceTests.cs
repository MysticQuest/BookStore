using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Infrastructure.Data;
using BookStore.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookStore.Application.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<AppDbContext> _dbContextMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _bookRepositoryMock = new Mock<IBookRepository>();
        _loggerMock = new Mock<ILogger<OrderService>>();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContextMock = new Mock<AppDbContext>(options) { CallBase = true };
        
        var transactionMock = new Mock<IDbContextTransaction>();
        var databaseFacadeMock = new Mock<DatabaseFacade>(_dbContextMock.Object);
        databaseFacadeMock.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionMock.Object);
        _dbContextMock.Setup(c => c.Database).Returns(databaseFacadeMock.Object);
        
        _sut = new OrderService(_orderRepositoryMock.Object, _bookRepositoryMock.Object, _dbContextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_CreatesOrderWithCorrectProperties()
    {
        // Arrange
        var request = new CreateOrderRequest { Address = "123 Main Street" };
        Order? capturedOrder = null;
        _orderRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => capturedOrder = order)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Address.Should().Be("123 Main Street");
        result.TotalCost.Should().Be(0);
        result.Id.Should().NotBeEmpty();
        capturedOrder.Should().NotBeNull();
        capturedOrder!.CreationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithClientProvidedId_UsesProvidedId()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new CreateOrderRequest { Id = clientId, Address = "123 Main Street" };
        Order? capturedOrder = null;
        
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);
        _orderRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => capturedOrder = order)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(clientId);
        capturedOrder.Should().NotBeNull();
        capturedOrder!.Id.Should().Be(clientId);
    }

    [Fact]
    public async Task CreateOrderAsync_WithExistingId_ReturnsExistingOrder_Idempotent()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        var existingOrder = new Order 
        { 
            Id = existingId, 
            Address = "Existing Address", 
            CreationDate = DateTime.UtcNow.AddMinutes(-5),
            TotalCost = 0 
        };
        var request = new CreateOrderRequest { Id = existingId, Address = "New Address" };
        
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(existingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        // Act
        var result = await _sut.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingId);
        result.Address.Should().Be("Existing Address"); // Returns existing, not new
        _orderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_WithoutId_GeneratesNewId()
    {
        // Arrange
        var request = new CreateOrderRequest { Address = "123 Main Street" }; // No Id provided
        Order? capturedOrder = null;
        
        _orderRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => capturedOrder = order)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        capturedOrder.Should().NotBeNull();
        capturedOrder!.Id.Should().NotBeEmpty();
        _orderRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddBookToOrderAsync_Fails_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new AddBookToOrderRequest { BookId = Guid.NewGuid(), Quantity = 1 };
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var (success, errorMessage) = await _sut.AddBookToOrderAsync(orderId, request);

        // Assert
        success.Should().BeFalse();
        errorMessage.Should().Contain("Order");
        errorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task AddBookToOrderAsync_Fails_WhenBookDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var order = new Order { Id = orderId, Address = "Test", TotalCost = 0 };
        var request = new AddBookToOrderRequest { BookId = bookId, Quantity = 1 };

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var (success, errorMessage) = await _sut.AddBookToOrderAsync(orderId, request);

        // Assert
        success.Should().BeFalse();
        errorMessage.Should().Contain("Book");
        errorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task AddBookToOrderAsync_Fails_WhenNoCopiesAvailable()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var order = new Order { Id = orderId, Address = "Test", TotalCost = 0 };
        var book = new Book { Id = bookId, Title = "Test Book", NumberOfCopies = 0, Price = 10.00m };
        var request = new AddBookToOrderRequest { BookId = bookId, Quantity = 1 };

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        // Act
        var (success, errorMessage) = await _sut.AddBookToOrderAsync(orderId, request);

        // Assert
        success.Should().BeFalse();
        errorMessage.Should().Contain("no copies available");
    }

    [Fact]
    public async Task AddBookToOrderAsync_Fails_WhenQuantityExceedsAvailable()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var order = new Order { Id = orderId, Address = "Test", TotalCost = 0 };
        var book = new Book { Id = bookId, Title = "Test Book", NumberOfCopies = 3, Price = 10.00m };
        var request = new AddBookToOrderRequest { BookId = bookId, Quantity = 5 };

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _orderRepositoryMock.Setup(r => r.GetOrderBookAsync(orderId, bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderBook?)null);

        // Act
        var (success, errorMessage) = await _sut.AddBookToOrderAsync(orderId, request);

        // Assert
        success.Should().BeFalse();
        errorMessage.Should().Contain("exceeds available copies");
    }

    [Fact]
    public async Task AddBookToOrderAsync_Succeeds_AndUpdatesInventory()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var order = new Order { Id = orderId, Address = "Test", TotalCost = 0 };
        var book = new Book { Id = bookId, Title = "Test Book", NumberOfCopies = 10, Price = 15.00m };
        var request = new AddBookToOrderRequest { BookId = bookId, Quantity = 3 };
        var newOrderBook = new OrderBook { OrderId = orderId, BookId = bookId, Quantity = 3, PriceAtPurchase = 15.00m };
        var orderWithBooks = new Order 
        { 
            Id = orderId, 
            Address = "Test", 
            TotalCost = 0,
            OrderBooks = new List<OrderBook> { newOrderBook }
        };

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _orderRepositoryMock.Setup(r => r.GetOrderBookAsync(orderId, bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderBook?)null);
        _orderRepositoryMock.Setup(r => r.GetByIdWithBooksAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderWithBooks);

        // Act
        var (success, errorMessage) = await _sut.AddBookToOrderAsync(orderId, request);

        // Assert
        success.Should().BeTrue();
        errorMessage.Should().BeNull();
        book.NumberOfCopies.Should().Be(7); // 10 - 3
        orderWithBooks.TotalCost.Should().Be(45.00m); // 3 * 15.00
        _orderRepositoryMock.Verify(r => r.AddOrderBookAsync(It.IsAny<OrderBook>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddBookToOrderAsync_UpdatesExistingOrderBook_WhenBookAlreadyInOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var order = new Order { Id = orderId, Address = "Test", TotalCost = 30.00m };
        var book = new Book { Id = bookId, Title = "Test Book", NumberOfCopies = 5, Price = 15.00m };
        var existingOrderBook = new OrderBook 
        { 
            OrderId = orderId, 
            BookId = bookId, 
            Quantity = 2, 
            PriceAtPurchase = 15.00m 
        };
        var request = new AddBookToOrderRequest { BookId = bookId, Quantity = 2 };
        var orderWithBooks = new Order 
        { 
            Id = orderId, 
            Address = "Test", 
            TotalCost = 30.00m,
            OrderBooks = new List<OrderBook> { existingOrderBook }
        };

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _orderRepositoryMock.Setup(r => r.GetOrderBookAsync(orderId, bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrderBook);
        _orderRepositoryMock.Setup(r => r.GetByIdWithBooksAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderWithBooks);

        // Act
        var (success, errorMessage) = await _sut.AddBookToOrderAsync(orderId, request);

        // Assert
        success.Should().BeTrue();
        existingOrderBook.Quantity.Should().Be(4); // 2 + 2
        book.NumberOfCopies.Should().Be(3); // 5 - 2
        orderWithBooks.TotalCost.Should().Be(60.00m); // 4 * 15 = 60
        _orderRepositoryMock.Verify(r => r.AddOrderBookAsync(It.IsAny<OrderBook>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveBookFromOrderAsync_RestoresInventory()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var order = new Order { Id = orderId, Address = "Test", TotalCost = 45.00m };
        var book = new Book { Id = bookId, Title = "Test Book", NumberOfCopies = 7 };
        var orderBook = new OrderBook 
        { 
            OrderId = orderId, 
            BookId = bookId, 
            Quantity = 3, 
            PriceAtPurchase = 15.00m 
        };
        var orderWithBooks = new Order 
        { 
            Id = orderId, 
            Address = "Test", 
            TotalCost = 45.00m,
            OrderBooks = new List<OrderBook>()
        };

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _orderRepositoryMock.Setup(r => r.GetOrderBookAsync(orderId, bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderBook);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _orderRepositoryMock.Setup(r => r.GetByIdWithBooksAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderWithBooks);

        // Act
        var result = await _sut.RemoveBookFromOrderAsync(orderId, bookId);

        // Assert
        result.Should().BeTrue();
        book.NumberOfCopies.Should().Be(10); // 7 + 3
        orderWithBooks.TotalCost.Should().Be(0); // No books left
        _orderRepositoryMock.Verify(r => r.RemoveOrderBook(orderBook), Times.Once);
    }

    [Fact]
    public async Task RemoveBookFromOrderAsync_ReturnsFalse_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _sut.RemoveBookFromOrderAsync(orderId, bookId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteOrderAsync_RestoresAllBookInventory()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var book1Id = Guid.NewGuid();
        var book2Id = Guid.NewGuid();
        var book1 = new Book { Id = book1Id, Title = "Book 1", NumberOfCopies = 5 };
        var book2 = new Book { Id = book2Id, Title = "Book 2", NumberOfCopies = 3 };
        var order = new Order 
        { 
            Id = orderId, 
            Address = "Test",
            OrderBooks = new List<OrderBook>
            {
                new() { OrderId = orderId, BookId = book1Id, Quantity = 2, PriceAtPurchase = 10.00m },
                new() { OrderId = orderId, BookId = book2Id, Quantity = 1, PriceAtPurchase = 20.00m }
            }
        };

        _orderRepositoryMock.Setup(r => r.GetByIdWithBooksAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book1);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book2);
        _orderRepositoryMock.Setup(r => r.DeleteAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteOrderAsync(orderId);

        // Assert
        result.Should().BeTrue();
        book1.NumberOfCopies.Should().Be(7); // 5 + 2
        book2.NumberOfCopies.Should().Be(4); // 3 + 1
        _orderRepositoryMock.Verify(r => r.DeleteAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
