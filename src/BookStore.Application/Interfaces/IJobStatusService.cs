using BookStore.Application.DTOs;

namespace BookStore.Application.Interfaces;

/// <summary>
/// Service to track job status.
/// </summary>
public interface IJobStatusService
{
    JobStatus GetStatus();
    void SetRunning(bool isRunning);
    void SetCompleted(DateTime executionTime, int booksAdded);
}
