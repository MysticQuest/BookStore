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
    private readonly IWebHostEnvironment _environment;

    public BooksController(IBookService bookService, IWebHostEnvironment environment)
    {
        _bookService = bookService;
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
        var books = await _bookService.GetAllBooksAsync(cancellationToken);
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
        return NoContent();
    }
}
