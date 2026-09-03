using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Exceptions;

public sealed class DeliveryNotFoundException(DeliveryId deliveryId)
    : Exception($"Delivery '{deliveryId}' was not found.")
{
    public DeliveryId DeliveryId { get; } = deliveryId;
}
