namespace RouteFlow.Deliveries.Contracts.IntegrationEvents;

public interface IDeliveriesIntegrationEvent
{
    Guid DeliveryId { get; }
}
