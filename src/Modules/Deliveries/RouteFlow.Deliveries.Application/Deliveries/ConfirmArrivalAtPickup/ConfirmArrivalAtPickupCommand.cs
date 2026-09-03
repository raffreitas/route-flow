using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.ConfirmArrivalAtPickup;

public sealed record ConfirmArrivalAtPickupCommand(DeliveryId DeliveryId);
