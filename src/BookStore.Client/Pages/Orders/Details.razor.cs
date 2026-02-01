using Microsoft.AspNetCore.Components;
using BookStore.Client.Services;
using BookStore.Client.ViewModels;

namespace BookStore.Client.Pages.Orders;

public partial class Details
{
    [Inject] private IOrderApiClient OrderApi { get; set; } = default!;
    [Inject] private IBookApiClient BookApi { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Parameter]
    public Guid OrderId { get; set; }

    private OrderViewModel? _order;
    private IEnumerable<OrderBookViewModel> _orderBooks = Enumerable.Empty<OrderBookViewModel>();
    private IEnumerable<BookViewModel>? _availableBooks;
    private AddBookToOrderViewModel _addBookModel = new();
    private bool _isLoading = true;
    private bool _isAdding = false;
    private string? _message;
    private bool _isError;

    protected override async Task OnInitializedAsync()
    {
        await LoadOrderData();
    }

    private async Task LoadOrderData()
    {
        _isLoading = true;
        try
        {
            var ordersTask = OrderApi.GetAllOrdersAsync();
            var orderBooksTask = OrderApi.GetOrderBooksAsync(OrderId);
            var booksTask = BookApi.GetAllBooksAsync();

            await Task.WhenAll(ordersTask, orderBooksTask, booksTask);

            var orders = await ordersTask;
            _order = orders.FirstOrDefault(o => o.Id == OrderId);
            _orderBooks = await orderBooksTask;
            
            var allBooks = await booksTask;
            _availableBooks = allBooks.Where(b => b.NumberOfCopies > 0).ToList();
        }
        catch (Exception ex)
        {
            ShowMessage($"Error loading order: {ex.Message}", true);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task AddBook()
    {
        if (_addBookModel.BookId == Guid.Empty)
        {
            ShowMessage("Please select a book.", true);
            return;
        }

        _isAdding = true;
        try
        {
            var (success, errorMessage) = await OrderApi.AddBookToOrderAsync(OrderId, _addBookModel);
            if (success)
            {
                ShowMessage("Book added to order!", false);
                _addBookModel = new AddBookToOrderViewModel();
                await LoadOrderData();
            }
            else
            {
                ShowMessage(errorMessage ?? "Failed to add book.", true);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Error: {ex.Message}", true);
        }
        finally
        {
            _isAdding = false;
        }
    }

    private async Task RemoveBook(Guid bookId)
    {
        try
        {
            var success = await OrderApi.RemoveBookFromOrderAsync(OrderId, bookId);
            if (success)
            {
                ShowMessage("Book removed from order.", false);
                await LoadOrderData();
            }
            else
            {
                ShowMessage("Failed to remove book.", true);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Error: {ex.Message}", true);
        }
    }

    private void GoBack()
    {
        Navigation.NavigateTo("/orders");
    }

    private void ShowMessage(string message, bool isError)
    {
        _message = message;
        _isError = isError;
    }

    private void ClearMessage()
    {
        _message = null;
    }
}
