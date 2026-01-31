namespace BookStore.Application.DTOs;

/// <summary>
/// Represents the status of the book fetch job.
/// </summary>
public class JobStatus
{
    public DateTime? LastExecutionTime { get; set; }
    public int? LastBooksAdded { get; set; }
    public bool IsRunning { get; set; }
}
