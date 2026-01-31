using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Infrastructure.Services;
using FluentAssertions;
using Moq;

namespace BookStore.Application.Tests.Services;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly BookService _sut;

    public BookServiceTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _sut = new BookService(_bookRepositoryMock.Object, _orderRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllBooksAsync_ReturnsBooks_WhenBooksExist()
    {
        // Arrange
        var books = new List<Book>
        {
            new() { Id = Guid.NewGuid(), Title = "Book 1", Number = 1, Price = 10.00m, NumberOfCopies = 5 },
            new() { Id = Guid.NewGuid(), Title = "Book 2", Number = 2, Price = 15.00m, NumberOfCopies = 3 }
        };
        _bookRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        // Act
        var result = await _sut.GetAllBooksAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Title.Should().Be("Book 1");
    }

    [Fact]
    public async Task GetAllBooksAsync_ReturnsEmptyList_WhenNoBooksExist()
    {
        // Arrange
        _bookRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Book>());

        // Act
        var result = await _sut.GetAllBooksAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBookByIdAsync_ReturnsBook_WhenBookExists()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = new Book { Id = bookId, Title = "Test Book", Number = 1, Price = 10.00m };
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        // Act
        var result = await _sut.GetBookByIdAsync(bookId);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Book");
    }

    [Fact]
    public async Task GetBookByIdAsync_ReturnsNull_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _sut.GetBookByIdAsync(bookId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateNumberOfCopiesAsync_ReturnsTrue_WhenBookExists()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepositoryMock.Setup(r => r.UpdateNumberOfCopiesAsync(bookId, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.UpdateNumberOfCopiesAsync(bookId, 10);

        // Assert
        result.Should().BeTrue();
        _bookRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateNumberOfCopiesAsync_ReturnsFalse_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepositoryMock.Setup(r => r.UpdateNumberOfCopiesAsync(bookId, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.UpdateNumberOfCopiesAsync(bookId, 10);

        // Assert
        result.Should().BeFalse();
        _bookRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePriceAsync_ReturnsTrue_WhenBookExists()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepositoryMock.Setup(r => r.UpdatePriceAsync(bookId, 25.99m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.UpdatePriceAsync(bookId, 25.99m);

        // Assert
        result.Should().BeTrue();
        _bookRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePriceAsync_ReturnsFalse_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepositoryMock.Setup(r => r.UpdatePriceAsync(bookId, 25.99m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.UpdatePriceAsync(bookId, 25.99m);

        // Assert
        result.Should().BeFalse();
        _bookRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteBookAsync_ReturnsFalse_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _sut.DeleteBookAsync(bookId);

        // Assert
        result.Should().BeFalse();
        _orderRepositoryMock.Verify(r => r.RemoveBookFromAllOrdersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteBookAsync_ReturnsTrue_WhenBookExists()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = new Book { Id = bookId, Title = "Test Book", Number = 1 };
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _bookRepositoryMock.Setup(r => r.DeleteAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteBookAsync(bookId);

        // Assert
        result.Should().BeTrue();
        _orderRepositoryMock.Verify(r => r.RemoveBookFromAllOrdersAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _bookRepositoryMock.Verify(r => r.DeleteAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
        _bookRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
