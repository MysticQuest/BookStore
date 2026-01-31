using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Infrastructure.Data;

namespace BookStore.Infrastructure.Services;

/// <summary>
/// Service for order management operations.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBookRepository _bookRepository;
    private readonly AppDbContext _dbContext;

    public OrderService(IOrderRepository orderRepository, IBookRepository bookRepository, AppDbContext dbContext)
    {
        _orderRepository = orderRepository;
        _bookRepository = bookRepository;
        _dbContext = dbContext;
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
        var order = new Order
        {
            Id = Guid.NewGuid(),
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
                existingOrderBook.Quantity += request.Quantity;
                book.NumberOfCopies -= request.Quantity;
                order.TotalCost += request.Quantity * existingOrderBook.PriceAtPurchase;
            }
            else
            {
                var orderBook = new OrderBook
                {
                    OrderId = orderId,
                    BookId = request.BookId,
                    Quantity = request.Quantity,
                    PriceAtPurchase = book.Price
                };

                await _orderRepository.AddOrderBookAsync(orderBook, cancellationToken);
                book.NumberOfCopies -= request.Quantity;
                order.TotalCost += request.Quantity * book.Price;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return (true, null);
        }
        catch
        {
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
                book.NumberOfCopies += orderBook.Quantity;
            }

            order.TotalCost -= orderBook.Quantity * orderBook.PriceAtPurchase;
            if (order.TotalCost < 0) order.TotalCost = 0;

            _orderRepository.RemoveOrderBook(orderBook);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return true;
        }
        catch
        {
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
        catch
        {
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
