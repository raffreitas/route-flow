using System.Diagnostics;
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
    OutboxMetrics metrics,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var batchStartedAt = Stopwatch.GetTimestamp();
            try
            {
                var processNextBatchImmediately = await ProcessPendingMessagesAsync(stoppingToken);
                if (processNextBatchImmediately)
                    continue;

                await Task.Delay(PollingInterval, timeProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                metrics.RecordProcessorError();
                logger.LogError(exception, "An error occurred while processing the deliveries outbox.");
                await Task.Delay(PollingInterval, timeProvider, stoppingToken);
            }
            finally
            {
                metrics.RecordBatchDuration(Stopwatch.GetElapsedTime(batchStartedAt));
            }
        }
    }

    private async Task<bool> ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DeliveriesDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();

        var retryBefore = timeProvider.GetUtcNow() - PollingInterval;

        var messages = await dbContext.OutboxMessages
            .TagWith("outbox-poll")
            .Where(message => message.ProcessedAt == null
                              && (message.LastAttemptAt == null || message.LastAttemptAt <= retryBefore))
            .OrderBy(message => message.OccurredAt)
            .ThenBy(message => message.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0) return false;

        metrics.RecordBatchSize(messages.Count);

        var contexts = messages
            .Select(message => TryGetActivityContext(
                message.TraceParent,
                message.TraceState))
            .ToArray();

        var links = contexts
            .Where(context => context.HasValue)
            .Select(context => new ActivityLink(context!.Value))
            .ToArray();

        using var batchActivity = DeliveriesActivitySource.Instance.StartActivity(
            "process outbox",
            ActivityKind.Internal,
            parentContext: default,
            links: links);

        batchActivity?.SetTag("outbox.message.count", messages.Count);
        batchActivity?.SetTag("outbox.batch.size", BatchSize);

        var processedCount = 0;
        var failedCount = 0;

        for (var index = 0; index < messages.Count; index++)
        {
            var message = messages[index];
            var parentContext = contexts[index];
            metrics.RecordMessageAge(timeProvider.GetUtcNow() - message.OccurredAt);
            try
            {
                using var messageActivity = StartMessageActivity(parentContext);
                messageActivity?.SetTag("outbox.message.type", message.Type);

                var envelope = IntegrationEventSerializer.DeserializeEnvelope(
                    message.Id,
                    message.Type,
                    message.OccurredAt,
                    message.Content);
                await publisher.PublishAsync(envelope, cancellationToken);
                message.MarkProcessed(timeProvider.GetUtcNow());
                metrics.RecordMessageProcessed();
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
                metrics.RecordMessageFailed();
                failedCount++;
                logger.LogWarning(
                    exception,
                    "Delivery outbox message {OutboxMessageId} could not be published.",
                    message.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        batchActivity?.SetTag("outbox.message.processed_count", processedCount);
        batchActivity?.SetTag("outbox.message.failed_count", failedCount);

        return messages.Count == BatchSize;
    }

    private static Activity? StartMessageActivity(ActivityContext? parentContext)
    {
        return DeliveriesActivitySource.Instance.StartActivity(
            "process outbox message",
            ActivityKind.Internal,
            parentContext ?? default(ActivityContext));
    }

    private static ActivityContext? TryGetActivityContext(string? traceParent, string? traceState)
    {
        if (string.IsNullOrWhiteSpace(traceParent))
        {
            return null;
        }

        return ActivityContext.TryParse(traceParent, traceState, isRemote: true, out var context)
            ? context
            : null;
    }
}
