using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.ReleaseDriverBeforePickup;

public sealed record ReleaseDriverBeforePickupCommand(DeliveryId DeliveryId, string Reason);
