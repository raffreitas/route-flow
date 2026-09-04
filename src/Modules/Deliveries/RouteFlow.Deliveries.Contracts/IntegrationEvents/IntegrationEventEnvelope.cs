namespace RouteFlow.Deliveries.Contracts.IntegrationEvents;

public sealed record IntegrationEventEnvelope<TEvent>(
    Guid MessageId,
    string EventType,
    DateTimeOffset OccurredAt,
    TEvent Event) : IIntegrationEventEnvelope
    where TEvent : IDeliveriesIntegrationEvent
{
    IDeliveriesIntegrationEvent IIntegrationEventEnvelope.Event => Event;
}
