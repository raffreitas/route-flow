using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.StartDispatchToPickup;

public sealed record StartDispatchToPickupCommand(DeliveryId DeliveryId);
