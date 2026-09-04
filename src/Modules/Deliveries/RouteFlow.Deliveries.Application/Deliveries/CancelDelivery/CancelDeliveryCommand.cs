using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.CancelDelivery;

public sealed record CancelDeliveryCommand(DeliveryId DeliveryId, string Reason);
