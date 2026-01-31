using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;

namespace BookStore.Infrastructure.Services;

/// <summary>
/// Service for order management operations.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBookRepository _bookRepository;

    public OrderService(IOrderRepository orderRepository, IBookRepository bookRepository)
    {
        _orderRepository = orderRepository;
        _bookRepository = bookRepository;
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
        // Get the order
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return (false, $"Order with ID '{orderId}' not found.");

        // Get the book
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book == null)
            return (false, $"Book with ID '{request.BookId}' not found.");

        // Check if book has copies available
        if (book.NumberOfCopies <= 0)
            return (false, $"Book '{book.Title}' has no copies available.");

        // Check if requested quantity exceeds available copies
        if (request.Quantity > book.NumberOfCopies)
            return (false, $"Requested quantity ({request.Quantity}) exceeds available copies ({book.NumberOfCopies}) for book '{book.Title}'.");

        // Check if book is already in the order
        var existingOrderBook = await _orderRepository.GetOrderBookAsync(orderId, request.BookId, cancellationToken);
        if (existingOrderBook != null)
        {
            // Update existing entry
            var totalQuantity = existingOrderBook.Quantity + request.Quantity;
            if (totalQuantity > book.NumberOfCopies + existingOrderBook.Quantity)
                return (false, $"Total quantity ({totalQuantity}) would exceed available copies ({book.NumberOfCopies + existingOrderBook.Quantity}) for book '{book.Title}'.");

            existingOrderBook.Quantity = totalQuantity;
            
            // Deduct copies from inventory
            book.NumberOfCopies -= request.Quantity;
            
            // Update order total
            order.TotalCost += request.Quantity * existingOrderBook.PriceAtPurchase;
        }
        else
        {
            // Create new order book entry
            var orderBook = new OrderBook
            {
                OrderId = orderId,
                BookId = request.BookId,
                Quantity = request.Quantity,
                PriceAtPurchase = book.Price
            };

            await _orderRepository.AddOrderBookAsync(orderBook, cancellationToken);
            
            // Deduct copies from inventory
            book.NumberOfCopies -= request.Quantity;
            
            // Update order total
            order.TotalCost += request.Quantity * book.Price;
        }

        await _orderRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<bool> RemoveBookFromOrderAsync(Guid orderId, Guid bookId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
            return false;

        var orderBook = await _orderRepository.GetOrderBookAsync(orderId, bookId, cancellationToken);
        if (orderBook == null)
            return false;

        // Restore copies to inventory
        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken);
        if (book != null)
        {
            book.NumberOfCopies += orderBook.Quantity;
        }

        // Update order total
        order.TotalCost -= orderBook.Quantity * orderBook.PriceAtPurchase;
        if (order.TotalCost < 0) order.TotalCost = 0;

        _orderRepository.RemoveOrderBook(orderBook);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Get order with books to restore inventory
        var order = await _orderRepository.GetByIdWithBooksAsync(id, cancellationToken);
        if (order == null)
            return false;

        // Restore copies for all books in the order
        foreach (var orderBook in order.OrderBooks)
        {
            var book = await _bookRepository.GetByIdAsync(orderBook.BookId, cancellationToken);
            if (book != null)
            {
                book.NumberOfCopies += orderBook.Quantity;
            }
        }

        await _orderRepository.DeleteAsync(id, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return true;
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
