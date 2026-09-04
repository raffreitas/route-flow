using RouteFlow.Deliveries.Contracts.IntegrationEvents;

namespace RouteFlow.Deliveries.Infrastructure.Messaging;

internal sealed class QueuedIntegrationEvent(IIntegrationEventEnvelope envelope)
{
    public IIntegrationEventEnvelope Envelope { get; } = envelope;

    public TaskCompletionSource Completion { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}
