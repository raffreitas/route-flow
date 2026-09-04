using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain.Events;

public sealed record DriverReleasedDomainEvent(
    DeliveryId DeliveryId,
    DriverId DriverId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
