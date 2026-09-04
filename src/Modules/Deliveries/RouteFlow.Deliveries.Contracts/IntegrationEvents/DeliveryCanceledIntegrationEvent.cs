namespace RouteFlow.Deliveries.Contracts.IntegrationEvents;

public sealed record DeliveryCanceledIntegrationEvent(
    Guid DeliveryId,
    string Reason) : IDeliveriesIntegrationEvent
{
    public const string EventType = "deliveries.delivery-canceled.v1";
}
