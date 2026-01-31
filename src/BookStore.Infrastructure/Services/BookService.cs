using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
using BookStore.Domain.Entities;

namespace BookStore.Infrastructure.Services;

/// <summary>
/// Service for book management operations.
/// </summary>
public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IOrderRepository _orderRepository;

    public BookService(IBookRepository bookRepository, IOrderRepository orderRepository)
    {
        _bookRepository = bookRepository;
        _orderRepository = orderRepository;
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

        await _orderRepository.RemoveBookFromAllOrdersAsync(id, cancellationToken);
        await _bookRepository.DeleteAsync(id, cancellationToken);
        await _bookRepository.SaveChangesAsync(cancellationToken);
        
        return true;
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
