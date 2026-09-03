using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain.Events;

public sealed record DriverAssignedDomainEvent(
    DeliveryId DeliveryId,
    DriverId DriverId,
    DateTimeOffset OccurredAt) : IDomainEvent;
