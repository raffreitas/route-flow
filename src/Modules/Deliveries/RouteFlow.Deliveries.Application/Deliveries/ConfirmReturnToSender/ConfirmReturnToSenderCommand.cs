using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.ConfirmReturnToSender;

public sealed record ConfirmReturnToSenderCommand(DeliveryId DeliveryId);
