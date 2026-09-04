using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.ConfirmAddressChangeInTransit;

public sealed record ConfirmAddressChangeInTransitCommand(
    DeliveryId DeliveryId,
    DeliveryAddress NewAddress);
