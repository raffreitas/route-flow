using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.RecordFailedAttempt;

public sealed record RecordFailedAttemptCommand(
    DeliveryId DeliveryId,
    FailureReason Reason,
    string? Notes = null);
