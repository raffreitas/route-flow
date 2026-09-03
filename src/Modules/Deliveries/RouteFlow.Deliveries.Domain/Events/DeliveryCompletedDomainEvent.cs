using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain.Events;

public sealed record DeliveryCompletedDomainEvent(
    DeliveryId DeliveryId,
    DateTimeOffset OccurredAt) : IDomainEvent;
