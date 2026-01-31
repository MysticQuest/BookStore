using BookStore.Domain.Entities;
using FluentAssertions;

namespace BookStore.Domain.Tests.Entities;

public class OrderBookTests
{
    [Fact]
    public void OrderBook_DefaultValues_AreCorrect()
    {
        // Act
        var orderBook = new OrderBook();

        // Assert
        orderBook.OrderId.Should().Be(Guid.Empty);
        orderBook.BookId.Should().Be(Guid.Empty);
        orderBook.Quantity.Should().Be(0);
        orderBook.PriceAtPurchase.Should().Be(0);
    }

    [Fact]
    public void OrderBook_CanSetProperties()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        // Act
        var orderBook = new OrderBook
        {
            OrderId = orderId,
            BookId = bookId,
            Quantity = 5,
            PriceAtPurchase = 15.99m
        };

        // Assert
        orderBook.OrderId.Should().Be(orderId);
        orderBook.BookId.Should().Be(bookId);
        orderBook.Quantity.Should().Be(5);
        orderBook.PriceAtPurchase.Should().Be(15.99m);
    }

    [Fact]
    public void OrderBook_NavigationProperties_CanBeAssigned()
    {
        // Arrange
        var order = new Order { Id = Guid.NewGuid(), Address = "Test Address" };
        var book = new Book { Id = Guid.NewGuid(), Title = "Test Book" };

        // Act
        var orderBook = new OrderBook
        {
            OrderId = order.Id,
            BookId = book.Id,
            Order = order,
            Book = book,
            Quantity = 2,
            PriceAtPurchase = 10.00m
        };

        // Assert
        orderBook.Order.Should().BeSameAs(order);
        orderBook.Book.Should().BeSameAs(book);
        orderBook.Order.Address.Should().Be("Test Address");
        orderBook.Book.Title.Should().Be("Test Book");
    }

    [Fact]
    public void OrderBook_PriceAtPurchase_CapturesHistoricalPrice()
    {
        // Arrange
        var book = new Book { Id = Guid.NewGuid(), Title = "Test Book", Price = 20.00m };
        
        // Capture the price at purchase time
        var orderBook = new OrderBook
        {
            BookId = book.Id,
            Book = book,
            Quantity = 1,
            PriceAtPurchase = book.Price
        };

        // Act - price changes after purchase
        book.Price = 25.00m;

        // Assert - PriceAtPurchase still has original value
        orderBook.PriceAtPurchase.Should().Be(20.00m);
        book.Price.Should().Be(25.00m);
    }
}
