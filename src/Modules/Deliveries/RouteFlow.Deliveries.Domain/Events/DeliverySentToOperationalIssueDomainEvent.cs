using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain.Events;

public sealed record DeliverySentToOperationalIssueDomainEvent(
    DeliveryId DeliveryId,
    FailureReason Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
