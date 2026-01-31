using System.Globalization;
using System.Net.Http.Json;
using BookStore.Application.Interfaces;
using BookStore.Application.Options;
using BookStore.Domain.Entities;
using BookStore.Infrastructure.External;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookStore.Infrastructure.Services;

/// <summary>
/// Service for fetching books from an external API and saving new ones to the database.
/// </summary>
public class BookFetchService : IBookFetchService
{
    private readonly HttpClient _httpClient;
    private readonly IBookRepository _bookRepository;
    private readonly BookFetchSettings _settings;
    private readonly ILogger<BookFetchService> _logger;

    public BookFetchService(
        HttpClient httpClient,
        IBookRepository bookRepository,
        IOptions<BookFetchSettings> settings,
        ILogger<BookFetchService> logger)
    {
        _httpClient = httpClient;
        _bookRepository = bookRepository;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<int> FetchAndSaveBooksAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching books from external API: {Url}", _settings.Url);

        var externalBooks = await _httpClient.GetFromJsonAsync<List<ExternalBookDto>>(
            _settings.Url,
            cancellationToken);

        if (externalBooks == null || externalBooks.Count == 0)
        {
            _logger.LogWarning("No books returned from external API");
            return 0;
        }

        _logger.LogInformation("Fetched {Count} books from external API", externalBooks.Count);

        var existingNumbers = (await _bookRepository.GetExistingBookNumbersAsync(cancellationToken))
            .ToHashSet();

        var newBooks = externalBooks
            .Where(eb => !existingNumbers.Contains(eb.Number))
            .Select(MapToEntity)
            .ToList();

        if (newBooks.Count == 0)
        {
            _logger.LogInformation("No new books to add - all books already exist in database");
            return 0;
        }

        await _bookRepository.AddRangeAsync(newBooks, cancellationToken);
        await _bookRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added {Count} new books to database", newBooks.Count);

        return newBooks.Count;
    }

    private static Book MapToEntity(ExternalBookDto dto)
    {
        return new Book
        {
            Id = Guid.NewGuid(),
            Number = dto.Number,
            Title = dto.Title,
            OriginalTitle = dto.OriginalTitle,
            ReleaseDate = DateTime.TryParseExact(dto.ReleaseDate, "MMM d, yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null,
            Description = dto.Description,
            Pages = dto.Pages,
            Cover = dto.Cover,
            Index = dto.Index,
            NumberOfCopies = 0,
            Price = 0
        };
    }
}
