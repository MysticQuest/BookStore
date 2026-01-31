using BookStore.Api.Hubs;
using BookStore.Application.Interfaces;

namespace BookStore.Api.Jobs;

/// <summary>
/// Hangfire job that fetches books from the external API on a schedule.
/// </summary>
public class BookFetchJob
{
    private readonly IBookFetchService _bookFetchService;
    private readonly IBookHubNotifier _bookHubNotifier;
    private readonly IJobStatusService _jobStatusService;
    private readonly ILogger<BookFetchJob> _logger;

    public BookFetchJob(
        IBookFetchService bookFetchService, 
        IBookHubNotifier bookHubNotifier,
        IJobStatusService jobStatusService,
        ILogger<BookFetchJob> logger)
    {
        _bookFetchService = bookFetchService;
        _bookHubNotifier = bookHubNotifier;
        _jobStatusService = jobStatusService;
        _logger = logger;
    }

    /// <summary>
    /// Executes the book fetch operation.
    /// </summary>
    public async Task ExecuteAsync()
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("Book fetch job started at {Time}", startTime);

        _jobStatusService.SetRunning(true);
        await _bookHubNotifier.NotifyJobStatusAsync(_jobStatusService.GetStatus());

        try
        {
            var booksAdded = await _bookFetchService.FetchAndSaveBooksAsync();
            var completionTime = DateTime.UtcNow;
            
            _logger.LogInformation("Book fetch job completed. {BooksAdded} new books added.", booksAdded);

            _jobStatusService.SetCompleted(completionTime, booksAdded);
            await _bookHubNotifier.NotifyJobStatusAsync(_jobStatusService.GetStatus());

            if (booksAdded > 0)
            {
                await _bookHubNotifier.NotifyBooksUpdatedAsync(booksAdded);
                _logger.LogInformation("Notified clients about {BooksAdded} new books.", booksAdded);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Book fetch job failed with error: {Message}", ex.Message);
            _jobStatusService.SetRunning(false);
            throw; // Re-throw to let Hangfire handle retry logic
        }
    }
}
