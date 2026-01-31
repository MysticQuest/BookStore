namespace BookStore.Client.ViewModels;

/// <summary>
/// View model for the result of a book fetch operation.
/// </summary>
public class FetchResultViewModel
{
    public string Message { get; set; } = string.Empty;
    public int BooksAdded { get; set; }
}
