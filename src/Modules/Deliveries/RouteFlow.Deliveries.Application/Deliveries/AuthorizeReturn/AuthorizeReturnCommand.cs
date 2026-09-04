using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.AuthorizeReturn;

public sealed record AuthorizeReturnCommand(DeliveryId DeliveryId, string Reason);
