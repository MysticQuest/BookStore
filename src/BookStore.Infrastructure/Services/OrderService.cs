using BookStore.Application.DTOs;
using BookStore.Application.Events;
using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace BookStore.Infrastructure.Services;

/// <summary>
/// Service for order management operations.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBookRepository _bookRepository;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<OrderService> _logger;

    public event EventHandler<OrderChangedEventArgs>? OrderChanged;

    public OrderService(
        IOrderRepository orderRepository,
        IBookRepository bookRepository,
        AppDbContext dbContext,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _bookRepository = bookRepository;
        _dbContext = dbContext;
        _logger = logger;
    }

    private async Task RefreshOrderPricesAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithBooksAsync(orderId, cancellationToken);
        if (order == null) return;

        decimal totalCost = 0;
        foreach (var orderBook in order.OrderBooks)
        {
            var book = await _bookRepository.GetByIdAsync(orderBook.BookId, cancellationToken);
            if (book != null)
            {
                orderBook.PriceAtPurchase = book.Price;
                totalCost += orderBook.Quantity * book.Price;
            }
        }
        order.TotalCost = totalCost;
    }

    private void RaiseOrderChanged(Guid orderId)
    {
        OrderChanged?.Invoke(this, new OrderChangedEventArgs(orderId));
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        return order == null ? null : MapToDto(order);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Id.HasValue)
        {
            var existing = await _orderRepository.GetByIdAsync(request.Id.Value, cancellationToken);
            if (existing != null)
            {
                _logger.LogDebug("Returning existing order {OrderId} for idempotent request", existing.Id);
                return MapToDto(existing);
            }
        }

        var order = new Order
        {
            Id = request.Id ?? Guid.NewGuid(),
            Address = request.Address,
            CreationDate = DateTime.UtcNow,
            TotalCost = 0
        };

        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderBookDto>> GetOrderBooksAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithBooksAsync(orderId, cancellationToken);

        if (order == null)
            return Enumerable.Empty<OrderBookDto>();

        return order.OrderBooks.Select(ob => new OrderBookDto
        {
            BookId = ob.BookId,
            BookTitle = ob.Book.Title,
            Quantity = ob.Quantity,
            PriceAtPurchase = ob.PriceAtPurchase
        });
    }

    public async Task<(bool Success, string? ErrorMessage)> AddBookToOrderAsync(
        Guid orderId,
        AddBookToOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return (false, $"Order with ID '{orderId}' not found.");

        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book == null)
            return (false, $"Book with ID '{request.BookId}' not found.");

        if (book.NumberOfCopies <= 0)
            return (false, $"Book '{book.Title}' has no copies available.");

        if (request.Quantity > book.NumberOfCopies)
            return (false, $"Requested quantity ({request.Quantity}) exceeds available copies ({book.NumberOfCopies}) for book '{book.Title}'.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var existingOrderBook = await _orderRepository.GetOrderBookAsync(orderId, request.BookId, cancellationToken);
            if (existingOrderBook != null)
            {
                _logger.LogDebug("Updating existing order book entry for order {OrderId}, book {BookId}", orderId, request.BookId);
                existingOrderBook.Quantity += request.Quantity;
            }
            else
            {
                _logger.LogDebug("Adding new book {BookId} to order {OrderId}", request.BookId, orderId);
                var orderBook = new OrderBook
                {
                    OrderId = orderId,
                    BookId = request.BookId,
                    Quantity = request.Quantity,
                    PriceAtPurchase = book.Price
                };

                await _orderRepository.AddOrderBookAsync(orderBook, cancellationToken);
            }

            book.NumberOfCopies -= request.Quantity;
            await RefreshOrderPricesAsync(orderId, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            RaiseOrderChanged(orderId);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add book {BookId} to order {OrderId}", request.BookId, orderId);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> RemoveBookFromOrderAsync(Guid orderId, Guid bookId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return false;

        var orderBook = await _orderRepository.GetOrderBookAsync(orderId, bookId, cancellationToken);
        if (orderBook == null)
            return false;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken);
            if (book != null)
            {
                _logger.LogDebug("Restoring {Quantity} copies to book {BookId}", orderBook.Quantity, bookId);
                book.NumberOfCopies += orderBook.Quantity;
            }

            _orderRepository.RemoveOrderBook(orderBook);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await RefreshOrderPricesAsync(orderId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            RaiseOrderChanged(orderId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove book {BookId} from order {OrderId}", bookId, orderId);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithBooksAsync(id, cancellationToken);
        if (order == null)
            return false;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _logger.LogDebug("Restoring inventory for {Count} books from order {OrderId}", order.OrderBooks.Count, id);
            foreach (var orderBook in order.OrderBooks)
            {
                var book = await _bookRepository.GetByIdAsync(orderBook.BookId, cancellationToken);
                if (book != null)
                {
                    book.NumberOfCopies += orderBook.Quantity;
                }
            }

            await _orderRepository.DeleteAsync(id, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete order {OrderId}", id);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            Address = order.Address,
            CreationDate = order.CreationDate,
            TotalCost = order.TotalCost
        };
    }
}
