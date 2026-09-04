using RouteFlow.Deliveries.Contracts.IntegrationEvents;

namespace RouteFlow.Deliveries.Infrastructure.Messaging;

internal interface IIntegrationEventPublisher
{
    Task PublishAsync(
        IIntegrationEventEnvelope envelope,
        CancellationToken cancellationToken = default);
}
