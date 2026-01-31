using BookStore.Application.DTOs;

namespace BookStore.Application.Interfaces;

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
