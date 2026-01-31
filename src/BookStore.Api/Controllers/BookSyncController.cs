using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Controllers;

/// <summary>
/// Controller for synchronizing books from external sources.
/// </summary>
[ApiController]
[Route("api/books")]
public class BookSyncController : ControllerBase
{
    private readonly IBookFetchService _bookFetchService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<BookSyncController> _logger;

    public BookSyncController(
        IBookFetchService bookFetchService,
        ICacheService cacheService,
        ILogger<BookSyncController> logger)
    {
        _bookFetchService = bookFetchService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Fetches books from the external API and saves new ones to the database.
    /// This is an idempotent operation - only books with new 'Number' values are added.
    /// </summary>
    [HttpPost("fetch")]
    [ProducesResponseType(typeof(FetchBooksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<FetchBooksResponse>> FetchBooks(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching books from external API");
        var count = await _bookFetchService.FetchAndSaveBooksAsync(cancellationToken);
        
        if (count > 0)
        {
            _cacheService.InvalidateBooksCache();
            _logger.LogInformation("Added {Count} new books from external API", count);
        }
        else
        {
            _logger.LogDebug("No new books to add from external API");
        }

        return Ok(FetchBooksResponse.Success(count));
    }
}
