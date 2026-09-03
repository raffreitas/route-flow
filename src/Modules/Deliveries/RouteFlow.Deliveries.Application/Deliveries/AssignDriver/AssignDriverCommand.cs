using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.AssignDriver;

public sealed record AssignDriverCommand(DeliveryId DeliveryId, DriverId DriverId);
