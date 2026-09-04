using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.ConfirmDeliveryToRecipient;

public sealed record ConfirmDeliveryToRecipientCommand(DeliveryId DeliveryId);
