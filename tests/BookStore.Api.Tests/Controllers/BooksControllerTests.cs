using BookStore.Api.Controllers;
using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookStore.Api.Tests.Controllers;

public class BooksControllerTests
{
    private readonly Mock<IBookService> _bookServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly Mock<ILogger<BooksController>> _loggerMock;
    private readonly BooksController _sut;

    public BooksControllerTests()
    {
        _bookServiceMock = new Mock<IBookService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _environmentMock = new Mock<IWebHostEnvironment>();
        _loggerMock = new Mock<ILogger<BooksController>>();

        _sut = new BooksController(
            _bookServiceMock.Object,
            _cacheServiceMock.Object,
            _environmentMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsOk_WithBooksFromCache()
    {
        // Arrange
        var cachedBooks = new List<BookDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Cached Book" }
        };
        _cacheServiceMock.Setup(c => c.Get<IEnumerable<BookDto>>(It.IsAny<string>()))
            .Returns(cachedBooks);

        // Act
        var result = await _sut.GetAllBooks(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().Be(cachedBooks);
        _bookServiceMock.Verify(s => s.GetAllBooksAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsOk_WithBooksFromDatabase_WhenNotCached()
    {
        // Arrange
        _cacheServiceMock.Setup(c => c.Get<IEnumerable<BookDto>>(It.IsAny<string>()))
            .Returns((IEnumerable<BookDto>?)null);

        var books = new List<BookDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Book 1" },
            new() { Id = Guid.NewGuid(), Title = "Book 2" }
        };
        _bookServiceMock.Setup(s => s.GetAllBooksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        // Act
        var result = await _sut.GetAllBooks(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().Be(books);
        _cacheServiceMock.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<IEnumerable<BookDto>>(), null), Times.Once);
    }

    [Fact]
    public async Task UpdateCopies_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var request = new UpdateCopiesRequest { NumberOfCopies = 10 };
        _bookServiceMock.Setup(s => s.UpdateNumberOfCopiesAsync(bookId, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.UpdateCopies(bookId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _cacheServiceMock.Verify(c => c.InvalidateBooksCache(), Times.Once);
    }

    [Fact]
    public async Task UpdateCopies_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var request = new UpdateCopiesRequest { NumberOfCopies = 10 };
        _bookServiceMock.Setup(s => s.UpdateNumberOfCopiesAsync(bookId, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.UpdateCopies(bookId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdatePrice_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var request = new UpdatePriceRequest { Price = 19.99m };
        _bookServiceMock.Setup(s => s.UpdatePriceAsync(bookId, 19.99m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.UpdatePrice(bookId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _cacheServiceMock.Verify(c => c.InvalidateBooksCache(), Times.Once);
    }

    [Fact]
    public async Task UpdatePrice_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var request = new UpdatePriceRequest { Price = 19.99m };
        _bookServiceMock.Setup(s => s.UpdatePriceAsync(bookId, 19.99m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.UpdatePrice(bookId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteBook_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookServiceMock.Setup(s => s.DeleteBookAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteBook(bookId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _cacheServiceMock.Verify(c => c.InvalidateBooksCache(), Times.Once);
        _cacheServiceMock.Verify(c => c.InvalidateOrdersCache(), Times.Once);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _bookServiceMock.Setup(s => s.DeleteBookAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteBook(bookId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteAllBooks_ReturnsNoContent_InDevelopmentEnvironment()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        var result = await _sut.DeleteAllBooks(CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _bookServiceMock.Verify(s => s.DeleteAllBooksAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAllBooks_ReturnsForbidden_InProductionEnvironment()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");

        // Act
        var result = await _sut.DeleteAllBooks(CancellationToken.None);

        // Assert
        var statusResult = result as ObjectResult;
        statusResult.Should().NotBeNull();
        statusResult!.StatusCode.Should().Be(403);
    }
}
