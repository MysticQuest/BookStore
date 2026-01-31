using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;
using BookStore.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace BookStore.Infrastructure.Services;

/// <summary>
/// Service for book management operations.
/// </summary>
public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<BookService> _logger;

    public BookService(
        IBookRepository bookRepository, 
        IOrderRepository orderRepository, 
        AppDbContext dbContext,
        ILogger<BookService> logger)
    {
        _bookRepository = bookRepository;
        _orderRepository = orderRepository;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync(CancellationToken cancellationToken = default)
    {
        var books = await _bookRepository.GetAllAsync(cancellationToken);
        return books.Select(MapToDto);
    }

    public async Task<BookDto?> GetBookByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
        return book == null ? null : MapToDto(book);
    }

    public async Task<bool> UpdateNumberOfCopiesAsync(Guid id, int numberOfCopies, CancellationToken cancellationToken = default)
    {
        var updated = await _bookRepository.UpdateNumberOfCopiesAsync(id, numberOfCopies, cancellationToken);
        
        if (updated)
        {
            await _bookRepository.SaveChangesAsync(cancellationToken);
        }
        
        return updated;
    }

    public async Task<bool> UpdatePriceAsync(Guid id, decimal price, CancellationToken cancellationToken = default)
    {
        var updated = await _bookRepository.UpdatePriceAsync(id, price, cancellationToken);
        
        if (updated)
        {
            await _bookRepository.SaveChangesAsync(cancellationToken);
        }
        
        return updated;
    }

    public async Task<bool> DeleteBookAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
        if (book == null)
            return false;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _logger.LogDebug("Removing book {BookId} from all orders", id);
            await _orderRepository.RemoveBookFromAllOrdersAsync(id, cancellationToken);
            await _bookRepository.DeleteAsync(id, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
            _logger.LogDebug("Book {BookId} deleted successfully", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete book {BookId}", id);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteAllBooksAsync(CancellationToken cancellationToken = default)
    {
        await _bookRepository.DeleteAllAsync(cancellationToken);
    }

    private static BookDto MapToDto(Book book)
    {
        return new BookDto
        {
            Id = book.Id,
            Number = book.Number,
            Title = book.Title,
            OriginalTitle = book.OriginalTitle,
            ReleaseDate = book.ReleaseDate,
            Description = book.Description,
            Pages = book.Pages,
            Cover = book.Cover,
            Index = book.Index,
            NumberOfCopies = book.NumberOfCopies,
            Price = book.Price
        };
    }
}
