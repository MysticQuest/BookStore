using BookStore.Api.Controllers;
using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookStore.Api.Tests.Controllers;

public class BookSyncControllerTests
{
    private readonly Mock<IBookFetchService> _bookFetchServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<BookSyncController>> _loggerMock;
    private readonly BookSyncController _sut;

    public BookSyncControllerTests()
    {
        _bookFetchServiceMock = new Mock<IBookFetchService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<BookSyncController>>();

        _sut = new BookSyncController(
            _bookFetchServiceMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task FetchBooks_ReturnsOk_WhenBooksAdded()
    {
        // Arrange
        _bookFetchServiceMock.Setup(s => s.FetchAndSaveBooksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _sut.FetchBooks(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        var response = okResult.Value as FetchBooksResponse;
        response.Should().NotBeNull();
        response!.BooksAdded.Should().Be(5);
        response.Message.Should().Contain("5");
    }

    [Fact]
    public async Task FetchBooks_ReturnsOk_WhenNoBooksToAdd()
    {
        // Arrange
        _bookFetchServiceMock.Setup(s => s.FetchAndSaveBooksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _sut.FetchBooks(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        var response = okResult.Value as FetchBooksResponse;
        response.Should().NotBeNull();
        response!.BooksAdded.Should().Be(0);
        response.Message.Should().Contain("No new books");
    }

    [Fact]
    public async Task FetchBooks_InvalidatesCache_WhenBooksAdded()
    {
        // Arrange
        _bookFetchServiceMock.Setup(s => s.FetchAndSaveBooksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        await _sut.FetchBooks(CancellationToken.None);

        // Assert
        _cacheServiceMock.Verify(c => c.InvalidateBooksCache(), Times.Once);
    }

    [Fact]
    public async Task FetchBooks_DoesNotInvalidateCache_WhenNoBooksAdded()
    {
        // Arrange
        _bookFetchServiceMock.Setup(s => s.FetchAndSaveBooksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        await _sut.FetchBooks(CancellationToken.None);

        // Assert
        _cacheServiceMock.Verify(c => c.InvalidateBooksCache(), Times.Never);
    }

    [Fact]
    public async Task FetchBooks_PropagatesCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _bookFetchServiceMock.Setup(s => s.FetchAndSaveBooksAsync(cts.Token))
            .ReturnsAsync(0);

        // Act
        await _sut.FetchBooks(cts.Token);

        // Assert
        _bookFetchServiceMock.Verify(s => s.FetchAndSaveBooksAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task FetchBooks_ThrowsException_WhenServiceFails()
    {
        // Arrange
        _bookFetchServiceMock.Setup(s => s.FetchAndSaveBooksAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("External API unavailable"));

        // Act
        var act = () => _sut.FetchBooks(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("External API unavailable");
    }
}
