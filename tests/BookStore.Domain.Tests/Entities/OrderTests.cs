using BookStore.Domain.Entities;
using FluentAssertions;

namespace BookStore.Domain.Tests.Entities;

public class OrderTests
{
    [Fact]
    public void Order_DefaultValues_AreCorrect()
    {
        // Act
        var order = new Order();

        // Assert
        order.Id.Should().Be(Guid.Empty);
        order.Address.Should().BeEmpty();
        order.CreationDate.Should().Be(default(DateTime));
        order.TotalCost.Should().Be(0);
        order.OrderBooks.Should().NotBeNull();
        order.OrderBooks.Should().BeEmpty();
    }

    [Fact]
    public void Order_CanSetProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var creationDate = DateTime.UtcNow;

        // Act
        var order = new Order
        {
            Id = id,
            Address = "123 Main Street, New York, NY 10001",
            CreationDate = creationDate,
            TotalCost = 59.97m
        };

        // Assert
        order.Id.Should().Be(id);
        order.Address.Should().Be("123 Main Street, New York, NY 10001");
        order.CreationDate.Should().Be(creationDate);
        order.TotalCost.Should().Be(59.97m);
    }

    [Fact]
    public void Order_OrderBooks_CanBeModified()
    {
        // Arrange
        var order = new Order { Id = Guid.NewGuid() };
        var orderBook = new OrderBook { OrderId = order.Id, Quantity = 3 };

        // Act
        order.OrderBooks.Add(orderBook);

        // Assert
        order.OrderBooks.Should().HaveCount(1);
        order.OrderBooks.Should().Contain(orderBook);
    }

    [Fact]
    public void Order_TotalCost_AcceptsDecimalPrecision()
    {
        // Arrange & Act
        var order = new Order
        {
            TotalCost = 123.45m
        };

        // Assert
        order.TotalCost.Should().Be(123.45m);
    }
}
