using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain.Events;

public sealed record DeliveryCanceledDomainEvent(
    DeliveryId DeliveryId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
