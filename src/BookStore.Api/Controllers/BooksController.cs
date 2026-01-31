using BookStore.Application.Constants;
using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

using static BookStore.Application.DTOs.ApiError;

namespace BookStore.Api.Controllers;

/// <summary>
/// Controller for book management operations (CRUD).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly ICacheService _cacheService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<BooksController> _logger;

    public BooksController(
        IBookService bookService, 
        ICacheService cacheService, 
        IWebHostEnvironment environment,
        ILogger<BooksController> logger)
    {
        _bookService = bookService;
        _cacheService = cacheService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all books from the database.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooks(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving all books");
        
        var cached = _cacheService.Get<IEnumerable<BookDto>>(CacheKeys.AllBooks);
        if (cached != null)
        {
            _logger.LogDebug("Returning books from cache");
            return Ok(cached);
        }

        var books = await _bookService.GetAllBooksAsync(cancellationToken);
        _cacheService.Set(CacheKeys.AllBooks, books);
        _logger.LogDebug("Retrieved {Count} books from database", books.Count());
        return Ok(books);
    }

    /// <summary>
    /// Updates the number of copies for a specific book.
    /// </summary>
    [HttpPut("{id:guid}/copies")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCopies(
        Guid id,
        [FromBody] UpdateCopiesRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating copies for book {BookId} to {Copies}", id, request.NumberOfCopies);
        var updated = await _bookService.UpdateNumberOfCopiesAsync(id, request.NumberOfCopies, cancellationToken);

        if (!updated)
        {
            _logger.LogWarning("Book {BookId} not found for copies update", id);
            return NotFound(ResourceNotFound("Book", id));
        }

        _cacheService.InvalidateBooksCache();
        return NoContent();
    }

    /// <summary>
    /// Updates the price for a specific book.
    /// </summary>
    [HttpPut("{id:guid}/price")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePrice(
        Guid id,
        [FromBody] UpdatePriceRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating price for book {BookId} to {Price}", id, request.Price);
        var updated = await _bookService.UpdatePriceAsync(id, request.Price, cancellationToken);

        if (!updated)
        {
            _logger.LogWarning("Book {BookId} not found for price update", id);
            return NotFound(ResourceNotFound("Book", id));
        }

        _cacheService.InvalidateBooksCache();
        return NoContent();
    }

    /// <summary>
    /// Deletes a specific book by its unique identifier.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteBook(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting book {BookId}", id);
        var deleted = await _bookService.DeleteBookAsync(id, cancellationToken);

        if (!deleted)
        {
            _logger.LogWarning("Book {BookId} not found for deletion", id);
            return NotFound(ResourceNotFound("Book", id));
        }

        _cacheService.InvalidateBooksCache();
        _cacheService.InvalidateOrdersCache();
        _logger.LogInformation("Book {BookId} deleted successfully", id);
        return NoContent();
    }

    /// <summary>
    /// Deletes all books from the database. Only available in Development environment.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAllBooks(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            _logger.LogWarning("Attempted to delete all books in non-development environment");
            return StatusCode(StatusCodes.Status403Forbidden, 
                WithMessage("This endpoint is only available in Development environment.", "FORBIDDEN"));
        }

        _logger.LogWarning("Deleting all books from database");
        await _bookService.DeleteAllBooksAsync(cancellationToken);
        _cacheService.InvalidateBooksCache();
        _cacheService.InvalidateOrdersCache();
        _logger.LogWarning("All books deleted from database");
        return NoContent();
    }
}
