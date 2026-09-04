using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.DispatchToNewRoute;

public sealed record DispatchToNewRouteCommand(DeliveryId DeliveryId, DriverId? DriverId = null);
