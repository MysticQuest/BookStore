using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Order entity using Entity Framework Core.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.CreationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByIdWithBooksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.OrderBooks)
                .ThenInclude(ob => ob.Book)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
    }

    public async Task<OrderBook?> GetOrderBookAsync(Guid orderId, Guid bookId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderBooks
            .Include(ob => ob.Book)
            .FirstOrDefaultAsync(ob => ob.OrderId == orderId && ob.BookId == bookId, cancellationToken);
    }

    public async Task AddOrderBookAsync(OrderBook orderBook, CancellationToken cancellationToken = default)
    {
        await _context.OrderBooks.AddAsync(orderBook, cancellationToken);
    }

    public void RemoveOrderBook(OrderBook orderBook)
    {
        _context.OrderBooks.Remove(orderBook);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.OrderBooks)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
            return false;

        _context.Orders.Remove(order);
        return true;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
