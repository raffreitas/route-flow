using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RouteFlow.Deliveries.Application.Observability;
using RouteFlow.Deliveries.Infrastructure.Persistence;

namespace RouteFlow.Deliveries.Infrastructure.Messaging;

internal sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processedMessages = await ProcessPendingMessagesAsync(stoppingToken);
                if (processedMessages) continue;
                await Task.Delay(PollingInterval, timeProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred while processing the deliveries outbox.");
                await Task.Delay(PollingInterval, timeProvider, stoppingToken);
            }
        }
    }

    private async Task<bool> ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DeliveriesDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();
        var messages = await dbContext.OutboxMessages
            .TagWith("outbox-poll")
            .Where(message => message.ProcessedAt == null)
            .OrderBy(message => message.OccurredAt)
            .ThenBy(message => message.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
            return false;

        using var activity = DeliveriesActivitySource.Instance.StartActivity("process delivery outbox");

        activity?.SetTag("outbox.message.count", messages.Count);
        activity?.SetTag("outbox.batch.size", BatchSize);

        var processedCount = 0;
        var failedCount = 0;

        foreach (var message in messages)
        {
            try
            {
                var envelope = IntegrationEventSerializer.DeserializeEnvelope(
                    message.Id,
                    message.Type,
                    message.OccurredAt,
                    message.Content);
                await publisher.PublishAsync(envelope, cancellationToken);
                message.MarkProcessed(timeProvider.GetUtcNow());
                processedCount++;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                var error = exception.GetBaseException().Message;
                message.MarkFailed(timeProvider.GetUtcNow(), error);
                failedCount++;
                logger.LogWarning(
                    exception,
                    "Delivery outbox message {OutboxMessageId} could not be published.",
                    message.Id);
            }
        }

        activity?.SetTag("outbox.message.processed_count", processedCount);
        activity?.SetTag("outbox.message.failed_count", failedCount);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}