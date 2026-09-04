using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain.Events;

public sealed record IncompatibleVehicleReportedDomainEvent(
    DeliveryId DeliveryId,
    DriverId DriverId,
    VehicleType RequiredVehicleType,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
