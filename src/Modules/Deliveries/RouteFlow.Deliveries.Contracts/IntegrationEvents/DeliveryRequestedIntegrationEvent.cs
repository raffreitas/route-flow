namespace RouteFlow.Deliveries.Contracts.IntegrationEvents;

public sealed record DeliveryRequestedIntegrationEvent(
    Guid DeliveryId,
    Guid MerchantId) : IDeliveriesIntegrationEvent
{
    public const string EventType = "deliveries.delivery-requested.v1";
}
