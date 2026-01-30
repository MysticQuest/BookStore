using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Book entity using Entity Framework Core.
/// </summary>
public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;

    public BookRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Books
            .AsNoTracking()
            .OrderBy(b => b.Number)
            .ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Books
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Book?> GetByNumberAsync(int number, CancellationToken cancellationToken = default)
    {
        return await _context.Books
            .FirstOrDefaultAsync(b => b.Number == number, cancellationToken);
    }

    public async Task<IEnumerable<int>> GetExistingBookNumbersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Books
            .AsNoTracking()
            .Select(b => b.Number)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Book> books, CancellationToken cancellationToken = default)
    {
        await _context.Books.AddRangeAsync(books, cancellationToken);
    }

    public async Task<bool> UpdatePriceAsync(Guid id, decimal price, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        
        if (book == null)
            return false;

        book.Price = price;
        return true;
    }

    public async Task<bool> UpdateNumberOfCopiesAsync(Guid id, int numberOfCopies, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        
        if (book == null)
            return false;

        book.NumberOfCopies = numberOfCopies;
        return true;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
