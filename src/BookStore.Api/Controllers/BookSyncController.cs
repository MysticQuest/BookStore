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
    private readonly ILogger<BookSyncController> _logger;

    public BookSyncController(
        IBookFetchService bookFetchService,
        ILogger<BookSyncController> logger)
    {
        _bookFetchService = bookFetchService;
        _logger = logger;
    }

    /// <summary>
    /// Fetches books from the external API and saves new ones to the database.
    /// This is an idempotent operation - only books with new 'Number' values are added.
    /// </summary>
    [HttpPost("fetch")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> FetchBooks(CancellationToken cancellationToken)
    {
        try
        {
            var count = await _bookFetchService.FetchAndSaveBooksAsync(cancellationToken);
            
            return Ok(new 
            { 
                message = count > 0 
                    ? $"Successfully added {count} new book(s)." 
                    : "No new books to add. All books already exist in the database.",
                booksAdded = count
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch books from external API");
            return StatusCode(StatusCodes.Status500InternalServerError, new 
            { 
                message = "Failed to fetch books from external API.",
                error = ex.Message
            });
        }
    }
}
