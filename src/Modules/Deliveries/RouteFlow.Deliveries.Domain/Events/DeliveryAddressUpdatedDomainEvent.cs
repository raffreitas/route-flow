using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain.Events;

public sealed record DeliveryAddressUpdatedDomainEvent(
    DeliveryId DeliveryId,
    DeliveryAddress Address,
    DateTimeOffset OccurredAt) : IDomainEvent;
