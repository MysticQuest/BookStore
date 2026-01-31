using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Controllers;

/// <summary>
/// Controller for order management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Retrieves all orders (summary only: Id, Address, TotalCost, CreationDate).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetAllOrdersAsync(cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Creates a new empty order.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var order = await _orderService.CreateOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetOrderBooks), new { id = order.Id }, order);
    }

    /// <summary>
    /// Retrieves the books for a specific order.
    /// </summary>
    [HttpGet("{id:guid}/books")]
    [ProducesResponseType(typeof(IEnumerable<OrderBookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderBookDto>>> GetOrderBooks(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
        if (order == null)
            return NotFound(new { message = $"Order with ID '{id}' not found." });

        var books = await _orderService.GetOrderBooksAsync(id, cancellationToken);
        return Ok(books);
    }

    /// <summary>
    /// Adds a book to an order.
    /// </summary>
    [HttpPost("{id:guid}/books")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddBookToOrder(
        Guid id,
        [FromBody] AddBookToOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, errorMessage) = await _orderService.AddBookToOrderAsync(id, request, cancellationToken);

        if (!success)
        {
            if (errorMessage?.Contains("not found") == true)
                return NotFound(new { message = errorMessage });
            
            return BadRequest(new { message = errorMessage });
        }

        return NoContent();
    }

    /// <summary>
    /// Removes a book from an order.
    /// </summary>
    [HttpDelete("{id:guid}/books/{bookId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveBookFromOrder(
        Guid id,
        Guid bookId,
        CancellationToken cancellationToken)
    {
        var removed = await _orderService.RemoveBookFromOrderAsync(id, bookId, cancellationToken);

        if (!removed)
            return NotFound(new { message = $"Order with ID '{id}' or book with ID '{bookId}' not found in the order." });

        return NoContent();
    }

    /// <summary>
    /// Deletes an order.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _orderService.DeleteOrderAsync(id, cancellationToken);

        if (!deleted)
            return NotFound(new { message = $"Order with ID '{id}' not found." });

        return NoContent();
    }
}
