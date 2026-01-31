namespace BookStore.Application.DTOs;

/// <summary>
/// Data transfer object for Order entity (summary view).
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public decimal TotalCost { get; set; }
}
