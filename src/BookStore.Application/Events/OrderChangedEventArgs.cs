namespace BookStore.Application.Events;

/// <summary>
/// Event arguments for when an order is modified.
/// </summary>
public class OrderChangedEventArgs : EventArgs
{
    /// <summary>
    /// The ID of the order that was changed.
    /// </summary>
    public Guid OrderId { get; }

    public OrderChangedEventArgs(Guid orderId)
    {
        OrderId = orderId;
    }
}
