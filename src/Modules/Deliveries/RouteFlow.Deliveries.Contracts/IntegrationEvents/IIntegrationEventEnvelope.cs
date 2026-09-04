namespace RouteFlow.Deliveries.Contracts.IntegrationEvents;

public interface IIntegrationEventEnvelope
{
    Guid MessageId { get; }
    string EventType { get; }
    DateTimeOffset OccurredAt { get; }
    IDeliveriesIntegrationEvent Event { get; }
}
