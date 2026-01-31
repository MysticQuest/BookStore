using BookStore.Application.Interfaces;
using Hangfire;

namespace BookStore.Api.Jobs;

/// <summary>
/// Hangfire job that fetches books from the external API on a schedule.
/// </summary>
public class BookFetchJob
{
    private readonly IBookFetchService _bookFetchService;
    private readonly IBookHubNotifier _bookHubNotifier;
    private readonly IJobStatusService _jobStatusService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<BookFetchJob> _logger;

    public BookFetchJob(
        IBookFetchService bookFetchService, 
        IBookHubNotifier bookHubNotifier,
        IJobStatusService jobStatusService,
        ICacheService cacheService,
        ILogger<BookFetchJob> logger)
    {
        _bookFetchService = bookFetchService;
        _bookHubNotifier = bookHubNotifier;
        _jobStatusService = jobStatusService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Executes the book fetch operation.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token provided by Hangfire when job is being cancelled.</param>
    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("Book fetch job started at {Time}", startTime);

        _jobStatusService.SetRunning(true);
        await _bookHubNotifier.NotifyJobStatusAsync(_jobStatusService.GetStatus(), cancellationToken);

        try
        {
            var booksAdded = await _bookFetchService.FetchAndSaveBooksAsync(cancellationToken);
            var completionTime = DateTime.UtcNow;
            
            _logger.LogInformation("Book fetch job completed. {BooksAdded} new books added.", booksAdded);

            _jobStatusService.SetCompleted(completionTime, booksAdded);
            await _bookHubNotifier.NotifyJobStatusAsync(_jobStatusService.GetStatus(), cancellationToken);

            if (booksAdded > 0)
            {
                _cacheService.InvalidateBooksCache();
                await _bookHubNotifier.NotifyBooksUpdatedAsync(booksAdded, cancellationToken);
                _logger.LogInformation("Notified clients about {BooksAdded} new books.", booksAdded);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Book fetch job was cancelled");
            _jobStatusService.SetRunning(false);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Book fetch job failed with error: {Message}", ex.Message);
            _jobStatusService.SetRunning(false);
            throw; // Re-throw to let Hangfire handle retry logic
        }
    }
}
