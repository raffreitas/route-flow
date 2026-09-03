using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain.Events;

public sealed record DeliveryReturnedToSenderDomainEvent(
    DeliveryId DeliveryId,
    DateTimeOffset OccurredAt) : IDomainEvent;
