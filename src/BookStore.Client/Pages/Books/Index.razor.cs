using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using BookStore.Client.Services;
using BookStore.Client.ViewModels;

namespace BookStore.Client.Pages.Books;

public partial class Index : IAsyncDisposable
{
    [Inject] private IBookApiClient BookApi { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private IEnumerable<BookViewModel>? _books;
    private bool _isLoading = true;
    private bool _isFetching = false;
    private bool _isSaving = false;
    private string? _message;
    private bool _isError = false;

    private Guid? _editingCopiesId;
    private int _editCopiesValue;
    private Guid? _editingPriceId;
    private decimal _editPriceValue;
    private Guid? _expandedDescriptionId;
    private HashSet<Guid> _truncatedBooks = new();
    private HubConnection? _hubConnection;
    private JobStatus? _jobStatus;

    private class JobStatus
    {
        public DateTime? LastExecutionTime { get; set; }
        public int? LastBooksAdded { get; set; }
        public bool IsRunning { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadBooks();
        await StartSignalRConnection();
    }

    private async Task StartSignalRConnection()
    {
        var hubUrl = Navigation.ToAbsoluteUri("/hubs/books");

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<int>("BooksUpdated", async (booksAdded) =>
        {
            ShowMessage($"{booksAdded} new book(s) added by scheduled job.", false);
            await LoadBooks();
            await InvokeAsync(StateHasChanged);
        });

        _hubConnection.On<JobStatus>("JobStatusUpdate", async (status) =>
        {
            _jobStatus = status;
            await InvokeAsync(StateHasChanged);
        });

        try
        {
            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR connection failed: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_books != null && _books.Any())
        {
            await DetectTruncatedDescriptions();
        }
    }

    private async Task DetectTruncatedDescriptions()
    {
        if (_books == null) return;

        var changed = false;
        foreach (var book in _books)
        {
            if (_expandedDescriptionId == book.Id) continue;
            
            try
            {
                var isTruncated = await JS.InvokeAsync<bool>("isTextTruncated", $"desc-{book.Id}");
                if (isTruncated && !_truncatedBooks.Contains(book.Id))
                {
                    _truncatedBooks.Add(book.Id);
                    changed = true;
                }
                else if (!isTruncated && _truncatedBooks.Contains(book.Id))
                {
                    _truncatedBooks.Remove(book.Id);
                    changed = true;
                }
            }
            catch
            {
            }
        }

        if (changed)
        {
            StateHasChanged();
        }
    }

    private async Task LoadBooks()
    {
        _isLoading = true;
        try
        {
            _books = await BookApi.GetAllBooksAsync();
        }
        catch (Exception ex)
        {
            ShowMessage($"Failed to load books: {ex.Message}", true);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task FetchBooksFromApi()
    {
        _isFetching = true;
        ClearMessage();
        try
        {
            var result = await BookApi.FetchBooksAsync();
            ShowMessage(result.Message, false);
            await LoadBooks();
        }
        catch (Exception ex)
        {
            ShowMessage($"Failed to fetch books: {ex.Message}", true);
        }
        finally
        {
            _isFetching = false;
        }
    }

    private void StartEditCopies(BookViewModel book)
    {
        CancelEditPrice();
        _editingCopiesId = book.Id;
        _editCopiesValue = book.NumberOfCopies;
    }

    private void CancelEditCopies()
    {
        _editingCopiesId = null;
        _editCopiesValue = 0;
    }

    private async Task SaveCopies(Guid bookId)
    {
        _isSaving = true;
        try
        {
            var success = await BookApi.UpdateCopiesAsync(bookId, _editCopiesValue);
            if (success)
            {
                var book = _books?.FirstOrDefault(b => b.Id == bookId);
                ShowMessage($"Copies updated successfully for book {book?.Number}!", false);
                await LoadBooks();
            }
            else
            {
                ShowMessage("Failed to update copies.", true);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Error: {ex.Message}", true);
        }
        finally
        {
            _isSaving = false;
            CancelEditCopies();
        }
    }

    private void StartEditPrice(BookViewModel book)
    {
        CancelEditCopies();
        _editingPriceId = book.Id;
        _editPriceValue = book.Price;
    }

    private void CancelEditPrice()
    {
        _editingPriceId = null;
        _editPriceValue = 0;
    }

    private async Task SavePrice(Guid bookId)
    {
        _isSaving = true;
        _editPriceValue = Math.Round(_editPriceValue, 2);
        try
        {
            var success = await BookApi.UpdatePriceAsync(bookId, _editPriceValue);
            if (success)
            {
                var book = _books?.FirstOrDefault(b => b.Id == bookId);
                ShowMessage($"Price updated successfully for book {book?.Number}!", false);
                await LoadBooks();
            }
            else
            {
                ShowMessage("Failed to update price.", true);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Error: {ex.Message}", true);
        }
        finally
        {
            _isSaving = false;
            CancelEditPrice();
        }
    }

    private void ShowMessage(string message, bool isError)
    {
        _message = message;
        _isError = isError;
    }

    private void ClearMessage()
    {
        _message = null;
        _isError = false;
    }

    private void ToggleDescription(Guid bookId)
    {
        _expandedDescriptionId = _expandedDescriptionId == bookId ? null : bookId;
    }

    private async Task RemoveBook(Guid bookId)
    {
        try
        {
            var success = await BookApi.DeleteBookAsync(bookId);
            if (success)
            {
                ShowMessage("Book deleted successfully!", false);
                await LoadBooks();
            }
            else
            {
                ShowMessage("Failed to delete book.", true);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Error: {ex.Message}", true);
        }
    }

    private static string TruncateDescription(string description, int maxLength)
    {
        if (string.IsNullOrEmpty(description) || description.Length <= maxLength)
            return description;
        
        return description.Substring(0, maxLength) + "...";
    }
}
