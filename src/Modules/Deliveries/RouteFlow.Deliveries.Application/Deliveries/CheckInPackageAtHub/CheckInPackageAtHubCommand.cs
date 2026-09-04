using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.CheckInPackageAtHub;

public sealed record CheckInPackageAtHubCommand(DeliveryId DeliveryId, HubId HubId);
