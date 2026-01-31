using BookStore.Application.Constants;
using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

    public BooksController(IBookService bookService, ICacheService cacheService, IWebHostEnvironment environment)
    {
        _bookService = bookService;
        _cacheService = cacheService;
        _environment = environment;
    }

    /// <summary>
    /// Retrieves all books from the database.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooks(CancellationToken cancellationToken)
    {
        var cached = _cacheService.Get<IEnumerable<BookDto>>(CacheKeys.AllBooks);
        if (cached != null)
            return Ok(cached);

        var books = await _bookService.GetAllBooksAsync(cancellationToken);
        _cacheService.Set(CacheKeys.AllBooks, books);
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
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _bookService.UpdateNumberOfCopiesAsync(id, request.NumberOfCopies, cancellationToken);

        if (!updated)
            return NotFound(new { message = $"Book with ID '{id}' not found." });

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
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _bookService.UpdatePriceAsync(id, request.Price, cancellationToken);

        if (!updated)
            return NotFound(new { message = $"Book with ID '{id}' not found." });

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
        var deleted = await _bookService.DeleteBookAsync(id, cancellationToken);

        if (!deleted)
            return NotFound(new { message = $"Book with ID '{id}' not found." });

        _cacheService.InvalidateBooksCache();
        _cacheService.InvalidateOrdersCache();
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
            return StatusCode(StatusCodes.Status403Forbidden, new 
            { 
                message = "This endpoint is only available in Development environment." 
            });
        }

        await _bookService.DeleteAllBooksAsync(cancellationToken);
        _cacheService.InvalidateBooksCache();
        _cacheService.InvalidateOrdersCache();
        return NoContent();
    }
}
