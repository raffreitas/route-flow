using RouteFlow.Deliveries.Contracts.IntegrationEvents;

namespace RouteFlow.Deliveries.Infrastructure.Messaging;

internal sealed class InProcessIntegrationEventPublisher(InProcessIntegrationEventQueue queue)
    : IIntegrationEventPublisher
{
    public async Task PublishAsync(
        IIntegrationEventEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        var queuedEvent = new QueuedIntegrationEvent(envelope);
        await queue.Writer.WriteAsync(queuedEvent, cancellationToken);
        await queuedEvent.Completion.Task.WaitAsync(cancellationToken);
    }
}
