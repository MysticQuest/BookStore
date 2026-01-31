using BookStore.Domain.Entities;
using FluentAssertions;

namespace BookStore.Domain.Tests.Entities;

public class BookTests
{
    [Fact]
    public void Book_DefaultValues_AreCorrect()
    {
        // Act
        var book = new Book();

        // Assert
        book.Id.Should().Be(Guid.Empty);
        book.Number.Should().Be(0);
        book.Title.Should().BeEmpty();
        book.OriginalTitle.Should().BeEmpty();
        book.ReleaseDate.Should().BeNull();
        book.Description.Should().BeEmpty();
        book.Pages.Should().Be(0);
        book.Cover.Should().BeEmpty();
        book.Index.Should().Be(0);
        book.NumberOfCopies.Should().Be(1);
        book.Price.Should().Be(0);
        book.OrderBooks.Should().NotBeNull();
        book.OrderBooks.Should().BeEmpty();
    }

    [Fact]
    public void Book_CanSetProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var releaseDate = new DateTime(2024, 1, 15);

        // Act
        var book = new Book
        {
            Id = id,
            Number = 1,
            Title = "Harry Potter",
            OriginalTitle = "Harry Potter and the Philosopher's Stone",
            ReleaseDate = releaseDate,
            Description = "A boy discovers he's a wizard",
            Pages = 320,
            Cover = "https://example.com/cover.jpg",
            Index = 0,
            NumberOfCopies = 10,
            Price = 19.99m
        };

        // Assert
        book.Id.Should().Be(id);
        book.Number.Should().Be(1);
        book.Title.Should().Be("Harry Potter");
        book.OriginalTitle.Should().Be("Harry Potter and the Philosopher's Stone");
        book.ReleaseDate.Should().Be(releaseDate);
        book.Description.Should().Be("A boy discovers he's a wizard");
        book.Pages.Should().Be(320);
        book.Cover.Should().Be("https://example.com/cover.jpg");
        book.Index.Should().Be(0);
        book.NumberOfCopies.Should().Be(10);
        book.Price.Should().Be(19.99m);
    }

    [Fact]
    public void Book_OrderBooks_CanBeModified()
    {
        // Arrange
        var book = new Book { Id = Guid.NewGuid() };
        var orderBook = new OrderBook { BookId = book.Id, Quantity = 2 };

        // Act
        book.OrderBooks.Add(orderBook);

        // Assert
        book.OrderBooks.Should().HaveCount(1);
        book.OrderBooks.Should().Contain(orderBook);
    }
}
