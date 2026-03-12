namespace Postech.Notifications.Api.Domain.Events;

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled
}

public class PaymentProcessedEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public PaymentStatus Status { get; set; }
}
