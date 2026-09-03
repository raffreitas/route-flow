using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.ConfirmPickup;

public sealed record ConfirmPickupCommand(DeliveryId DeliveryId, DriverId DriverId);
