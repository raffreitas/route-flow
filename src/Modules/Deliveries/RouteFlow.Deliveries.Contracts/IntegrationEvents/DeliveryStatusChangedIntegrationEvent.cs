namespace RouteFlow.Deliveries.Contracts.IntegrationEvents;

public sealed record DeliveryStatusChangedIntegrationEvent(
    Guid DeliveryId,
    string? PreviousStatus,
    string Status,
    string Custody,
    Guid? AssignedDriverId) : IDeliveriesIntegrationEvent
{
    public const string EventType = "deliveries.delivery-status-changed.v1";
}
