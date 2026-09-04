namespace RouteFlow.Deliveries.Contracts.IntegrationEvents;

public interface IDeliveriesIntegrationEventHandler<TEvent>
    where TEvent : IDeliveriesIntegrationEvent
{
    Task HandleAsync(
        IntegrationEventEnvelope<TEvent> envelope,
        CancellationToken cancellationToken = default);
}
