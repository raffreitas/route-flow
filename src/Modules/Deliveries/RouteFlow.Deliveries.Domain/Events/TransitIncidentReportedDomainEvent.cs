using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain.Events;

public sealed record TransitIncidentReportedDomainEvent(
    DeliveryId DeliveryId,
    DriverId DriverId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
