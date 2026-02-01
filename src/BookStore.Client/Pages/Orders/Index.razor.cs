using Microsoft.AspNetCore.Components;
using BookStore.Client.Services;
using BookStore.Client.ViewModels;

namespace BookStore.Client.Pages.Orders;

public partial class Index
{
    [Inject] private IOrderApiClient OrderApi { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private IEnumerable<OrderViewModel>? _orders;
    private bool _isLoading = true;
    private bool _showCreateForm = false;
    private bool _isCreating = false;
    private CreateOrderViewModel _createModel = new();
    private string? _message;
    private bool _isError;

    protected override async Task OnInitializedAsync()
    {
        await LoadOrders();
    }

    private async Task LoadOrders()
    {
        _isLoading = true;
        try
        {
            _orders = await OrderApi.GetAllOrdersAsync();
        }
        catch (Exception ex)
        {
            ShowMessage($"Error loading orders: {ex.Message}", true);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void ShowCreateForm()
    {
        _createModel = new CreateOrderViewModel();
        _showCreateForm = true;
    }

    private void HideCreateForm()
    {
        _showCreateForm = false;
    }

    private async Task CreateOrder()
    {
        _isCreating = true;
        try
        {
            var order = await OrderApi.CreateOrderAsync(_createModel);
            if (order != null)
            {
                ShowMessage("Order created successfully!", false);
                _showCreateForm = false;
                await LoadOrders();
            }
            else
            {
                ShowMessage("Failed to create order.", true);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Error: {ex.Message}", true);
        }
        finally
        {
            _isCreating = false;
        }
    }

    private void ViewDetails(Guid orderId)
    {
        Navigation.NavigateTo($"/orders/{orderId}");
    }

    private async Task DeleteOrder(Guid orderId)
    {
        try
        {
            var success = await OrderApi.DeleteOrderAsync(orderId);
            if (success)
            {
                ShowMessage("Order deleted successfully!", false);
                await LoadOrders();
            }
            else
            {
                ShowMessage("Failed to delete order.", true);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Error: {ex.Message}", true);
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
    }
}
