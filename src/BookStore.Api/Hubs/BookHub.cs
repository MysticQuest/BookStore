using Microsoft.AspNetCore.SignalR;

namespace BookStore.Api.Hubs;

/// <summary>
/// SignalR hub for real-time book update notifications.
/// </summary>
public class BookHub : Hub
{
    private readonly IJobStatusService _jobStatusService;

    public BookHub(IJobStatusService jobStatusService)
    {
        _jobStatusService = jobStatusService;
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var status = _jobStatusService.GetStatus();
        await Clients.Caller.SendAsync("JobStatusUpdate", status);
        await base.OnConnectedAsync();
    }
}

/// <summary>
/// Represents the status of the book fetch job.
/// </summary>
public class JobStatus
{
    public DateTime? LastExecutionTime { get; set; }
    public int? LastBooksAdded { get; set; }
    public bool IsRunning { get; set; }
}

/// <summary>
/// Service to track job status.
/// </summary>
public interface IJobStatusService
{
    JobStatus GetStatus();
    void SetRunning(bool isRunning);
    void SetCompleted(DateTime executionTime, int booksAdded);
}

/// <summary>
/// In-memory implementation of job status tracking.
/// </summary>
public class JobStatusService : IJobStatusService
{
    private readonly object _lock = new();
    private DateTime? _lastExecutionTime;
    private int? _lastBooksAdded;
    private bool _isRunning;

    public JobStatus GetStatus()
    {
        lock (_lock)
        {
            return new JobStatus
            {
                LastExecutionTime = _lastExecutionTime,
                LastBooksAdded = _lastBooksAdded,
                IsRunning = _isRunning
            };
        }
    }

    public void SetRunning(bool isRunning)
    {
        lock (_lock)
        {
            _isRunning = isRunning;
        }
    }

    public void SetCompleted(DateTime executionTime, int booksAdded)
    {
        lock (_lock)
        {
            _lastExecutionTime = executionTime;
            _lastBooksAdded = booksAdded;
            _isRunning = false;
        }
    }
}

/// <summary>
/// Interface for sending book notifications to clients.
/// </summary>
public interface IBookHubNotifier
{
    /// <summary>
    /// Notifies all connected clients that books have been updated.
    /// </summary>
    Task NotifyBooksUpdatedAsync(int booksAdded);

    /// <summary>
    /// Notifies all connected clients about the job status.
    /// </summary>
    Task NotifyJobStatusAsync(JobStatus status);
}

/// <summary>
/// Implementation for sending book notifications via SignalR.
/// </summary>
public class BookHubNotifier : IBookHubNotifier
{
    private readonly IHubContext<BookHub> _hubContext;

    public BookHubNotifier(IHubContext<BookHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyBooksUpdatedAsync(int booksAdded)
    {
        await _hubContext.Clients.All.SendAsync("BooksUpdated", booksAdded);
    }

    public async Task NotifyJobStatusAsync(JobStatus status)
    {
        await _hubContext.Clients.All.SendAsync("JobStatusUpdate", status);
    }
}
