using RouteFlow.Deliveries.Contracts.IntegrationEvents;

namespace RouteFlow.Deliveries.IntegrationTests;

internal sealed class RetryOnceDeliveryRequestedHandler(Guid expectedDeliveryId)
    : IDeliveriesIntegrationEventHandler<DeliveryRequestedIntegrationEvent>
{
    private int _attemptCount;
    private readonly TaskCompletionSource<IntegrationEventEnvelope<DeliveryRequestedIntegrationEvent>> _completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<IntegrationEventEnvelope<DeliveryRequestedIntegrationEvent>> Completion => _completion.Task;

    public Task HandleAsync(
        IntegrationEventEnvelope<DeliveryRequestedIntegrationEvent> envelope,
        CancellationToken cancellationToken = default)
    {
        if (envelope.Event.DeliveryId != expectedDeliveryId)
        {
            return Task.CompletedTask;
        }

        if (Interlocked.Increment(ref _attemptCount) == 1)
        {
            throw new InvalidOperationException("Transient handler failure.");
        }

        _completion.TrySetResult(envelope);
        return Task.CompletedTask;
    }
}
