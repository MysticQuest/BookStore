using System.Net;
using System.Text.Json;
using BookStore.Application.Interfaces;
using BookStore.Application.Options;
using BookStore.Domain.Entities;
using BookStore.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using MsOptions = Microsoft.Extensions.Options;

namespace BookStore.Application.Tests.Services;

public class BookFetchServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<ILogger<BookFetchService>> _loggerMock;
    private readonly BookFetchSettings _settings;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;

    public BookFetchServiceTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _loggerMock = new Mock<ILogger<BookFetchService>>();
        _settings = new BookFetchSettings { Url = "https://api.example.com/books" };
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
    }

    private BookFetchService CreateService()
    {
        return new BookFetchService(
            _httpClient,
            _bookRepositoryMock.Object,
            MsOptions.Options.Create(_settings),
            _loggerMock.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, object? content)
    {
        var response = new HttpResponseMessage(statusCode);
        if (content != null)
        {
            response.Content = new StringContent(
                JsonSerializer.Serialize(content),
                System.Text.Encoding.UTF8,
                "application/json");
        }

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    [Fact]
    public async Task FetchAndSaveBooksAsync_ReturnsZero_WhenApiReturnsEmptyList()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, new List<object>());
        var sut = CreateService();

        // Act
        var result = await sut.FetchAndSaveBooksAsync();

        // Assert
        result.Should().Be(0);
        _bookRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Book>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FetchAndSaveBooksAsync_ReturnsZero_WhenApiReturnsNull()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
        };
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var sut = CreateService();

        // Act
        var result = await sut.FetchAndSaveBooksAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task FetchAndSaveBooksAsync_AddsOnlyNewBooks_WhenSomeAlreadyExist()
    {
        // Arrange
        var externalBooks = new[]
        {
            new { number = 1, title = "Book 1", originalTitle = "", releaseDate = "", description = "", pages = 100, cover = "", index = 1 },
            new { number = 2, title = "Book 2", originalTitle = "", releaseDate = "", description = "", pages = 200, cover = "", index = 2 },
            new { number = 3, title = "Book 3", originalTitle = "", releaseDate = "", description = "", pages = 300, cover = "", index = 3 }
        };
        SetupHttpResponse(HttpStatusCode.OK, externalBooks);
        
        _bookRepositoryMock.Setup(r => r.GetExistingBookNumbersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1, 2 });

        var sut = CreateService();

        // Act
        var result = await sut.FetchAndSaveBooksAsync();

        // Assert
        result.Should().Be(1);
        _bookRepositoryMock.Verify(r => r.AddRangeAsync(
            It.Is<IEnumerable<Book>>(books => books.Count() == 1 && books.First().Number == 3),
            It.IsAny<CancellationToken>()), Times.Once);
        _bookRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FetchAndSaveBooksAsync_AddsAllBooks_WhenNoneExist()
    {
        // Arrange
        var externalBooks = new[]
        {
            new { number = 1, title = "Book 1", originalTitle = "", releaseDate = "", description = "", pages = 100, cover = "", index = 1 },
            new { number = 2, title = "Book 2", originalTitle = "", releaseDate = "", description = "", pages = 200, cover = "", index = 2 }
        };
        SetupHttpResponse(HttpStatusCode.OK, externalBooks);
        
        _bookRepositoryMock.Setup(r => r.GetExistingBookNumbersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());

        var sut = CreateService();

        // Act
        var result = await sut.FetchAndSaveBooksAsync();

        // Assert
        result.Should().Be(2);
        _bookRepositoryMock.Verify(r => r.AddRangeAsync(
            It.Is<IEnumerable<Book>>(books => books.Count() == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FetchAndSaveBooksAsync_ReturnsZero_WhenAllBooksAlreadyExist()
    {
        // Arrange
        var externalBooks = new[]
        {
            new { number = 1, title = "Book 1", originalTitle = "", releaseDate = "", description = "", pages = 100, cover = "", index = 1 },
            new { number = 2, title = "Book 2", originalTitle = "", releaseDate = "", description = "", pages = 200, cover = "", index = 2 }
        };
        SetupHttpResponse(HttpStatusCode.OK, externalBooks);
        
        _bookRepositoryMock.Setup(r => r.GetExistingBookNumbersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1, 2 });

        var sut = CreateService();

        // Act
        var result = await sut.FetchAndSaveBooksAsync();

        // Assert
        result.Should().Be(0);
        _bookRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Book>>(), It.IsAny<CancellationToken>()), Times.Never);
        _bookRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FetchAndSaveBooksAsync_ParsesReleaseDate_WhenValidFormat()
    {
        // Arrange
        var externalBooks = new[]
        {
            new { number = 1, title = "Book 1", originalTitle = "", releaseDate = "Jun 26, 1997", description = "", pages = 100, cover = "", index = 1 }
        };
        SetupHttpResponse(HttpStatusCode.OK, externalBooks);
        
        _bookRepositoryMock.Setup(r => r.GetExistingBookNumbersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());

        Book? capturedBook = null;
        _bookRepositoryMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Book>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Book>, CancellationToken>((books, _) => capturedBook = books.First());

        var sut = CreateService();

        // Act
        await sut.FetchAndSaveBooksAsync();

        // Assert
        capturedBook.Should().NotBeNull();
        capturedBook!.ReleaseDate.Should().Be(new DateTime(1997, 6, 26));
    }

    [Fact]
    public async Task FetchAndSaveBooksAsync_SetsReleaseDateToNull_WhenInvalidFormat()
    {
        // Arrange
        var externalBooks = new[]
        {
            new { number = 1, title = "Book 1", originalTitle = "", releaseDate = "invalid-date", description = "", pages = 100, cover = "", index = 1 }
        };
        SetupHttpResponse(HttpStatusCode.OK, externalBooks);
        
        _bookRepositoryMock.Setup(r => r.GetExistingBookNumbersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());

        Book? capturedBook = null;
        _bookRepositoryMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Book>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Book>, CancellationToken>((books, _) => capturedBook = books.First());

        var sut = CreateService();

        // Act
        await sut.FetchAndSaveBooksAsync();

        // Assert
        capturedBook.Should().NotBeNull();
        capturedBook!.ReleaseDate.Should().BeNull();
    }

    [Fact]
    public async Task FetchAndSaveBooksAsync_SetsDefaultValues_ForNewBooks()
    {
        // Arrange
        var externalBooks = new[]
        {
            new { number = 1, title = "Test Book", originalTitle = "Original", releaseDate = "", description = "Desc", pages = 150, cover = "cover.jpg", index = 1 }
        };
        SetupHttpResponse(HttpStatusCode.OK, externalBooks);
        
        _bookRepositoryMock.Setup(r => r.GetExistingBookNumbersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());

        Book? capturedBook = null;
        _bookRepositoryMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Book>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Book>, CancellationToken>((books, _) => capturedBook = books.First());

        var sut = CreateService();

        // Act
        await sut.FetchAndSaveBooksAsync();

        // Assert
        capturedBook.Should().NotBeNull();
        capturedBook!.NumberOfCopies.Should().Be(1);
        capturedBook.Price.Should().Be(0);
        capturedBook.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task FetchAndSaveBooksAsync_ThrowsHttpRequestException_WhenApiCallFails()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var sut = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => sut.FetchAndSaveBooksAsync());
    }
}
