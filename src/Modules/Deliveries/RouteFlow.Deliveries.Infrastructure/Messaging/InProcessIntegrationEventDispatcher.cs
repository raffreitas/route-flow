using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RouteFlow.Deliveries.Contracts.IntegrationEvents;

namespace RouteFlow.Deliveries.Infrastructure.Messaging;

internal sealed class InProcessIntegrationEventDispatcher(
    InProcessIntegrationEventQueue queue,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var queuedEvent in queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await DispatchAsync(queuedEvent.Envelope, stoppingToken);
                queuedEvent.Completion.TrySetResult();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                queuedEvent.Completion.TrySetCanceled(stoppingToken);
                break;
            }
            catch (Exception exception)
            {
                queuedEvent.Completion.TrySetException(exception);
            }
        }
    }

    private async Task DispatchAsync(
        IIntegrationEventEnvelope envelope,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var eventType = envelope.Event.GetType();
        var handlerContract = typeof(IDeliveriesIntegrationEventHandler<>)
            .MakeGenericType(eventType);
        var handleMethod = handlerContract.GetMethod(nameof(
            IDeliveriesIntegrationEventHandler<IDeliveriesIntegrationEvent>.HandleAsync))!;

        foreach (var handler in scope.ServiceProvider.GetServices(handlerContract))
        {
            var task = (Task)handleMethod.Invoke(handler, [envelope, cancellationToken])!;
            await task;
        }
    }
}
