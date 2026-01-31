using BookStore.Application.DTOs;
using BookStore.Application.Interfaces;
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
/// Implementation for sending book notifications via SignalR.
/// </summary>
public class BookHubNotifier : IBookHubNotifier
{
    private readonly IHubContext<BookHub> _hubContext;

    public BookHubNotifier(IHubContext<BookHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyBooksUpdatedAsync(int booksAdded, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync("BooksUpdated", booksAdded, cancellationToken);
    }

    public async Task NotifyJobStatusAsync(JobStatus status, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync("JobStatusUpdate", status, cancellationToken);
    }
}
