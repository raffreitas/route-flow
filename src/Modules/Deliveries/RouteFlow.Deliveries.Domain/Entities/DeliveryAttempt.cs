using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Domain.Entities;

public sealed class DeliveryAttempt
{
    public Guid Id { get; private set; }
    public int AttemptNumber { get; private set; }
    public FailureReason Reason { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string? Notes { get; private set; }

    internal DeliveryAttempt(int attemptNumber, FailureReason reason, DateTimeOffset occurredAt, string? notes = null)
    {
        Id = Guid.CreateVersion7(occurredAt);
        AttemptNumber = attemptNumber;
        Reason = reason;
        OccurredAt = occurredAt;
        Notes = notes;
    }
}