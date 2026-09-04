using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.UpdateAddressBeforePickup;

public sealed record UpdateAddressBeforePickupCommand(
    DeliveryId DeliveryId,
    DeliveryAddress NewAddress);
