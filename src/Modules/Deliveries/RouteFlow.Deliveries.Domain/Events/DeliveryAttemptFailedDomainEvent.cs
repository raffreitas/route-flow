using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain.Events;

public sealed record DeliveryAttemptFailedDomainEvent(
    DeliveryId DeliveryId,
    int AttemptNumber,
    FailureReason Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
