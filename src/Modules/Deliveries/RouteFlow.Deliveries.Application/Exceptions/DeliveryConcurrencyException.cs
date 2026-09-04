using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Exceptions;

public sealed class DeliveryConcurrencyException(DeliveryId deliveryId, Exception innerException)
    : Exception($"Delivery '{deliveryId}' was modified by another operation.", innerException)
{
    public DeliveryId DeliveryId { get; } = deliveryId;
}
