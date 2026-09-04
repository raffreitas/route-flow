using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RouteFlow.Deliveries.Infrastructure.Persistence;

namespace RouteFlow.Deliveries.Infrastructure.Messaging;

internal sealed class DeliveriesOutboxHealthCheck(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider) : IHealthCheck
{
    private static readonly TimeSpan MaximumPendingAge = TimeSpan.FromMinutes(5);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DeliveriesDbContext>();
            var state = await dbContext.OutboxMessages
                .Where(message => message.ProcessedAt == null)
                .GroupBy(_ => 1)
                .Select(messages => new
                {
                    PendingCount = messages.Count(),
                    FailedCount = messages.Count(message => message.Error != null),
                    OldestOccurredAt = messages.Min(message => message.OccurredAt)
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (state is null)
            {
                return HealthCheckResult.Healthy("The deliveries outbox has no pending messages.");
            }

            var oldestPendingAge = timeProvider.GetUtcNow() - state.OldestOccurredAt;
            var data = new Dictionary<string, object>
            {
                ["pending_count"] = state.PendingCount,
                ["failed_count"] = state.FailedCount,
                ["oldest_pending_age_seconds"] = Math.Max(0, oldestPendingAge.TotalSeconds)
            };

            return oldestPendingAge > MaximumPendingAge
                ? HealthCheckResult.Degraded(
                    $"The oldest deliveries outbox message has been pending for more than {MaximumPendingAge.TotalMinutes:0} minutes.",
                    data: data)
                : HealthCheckResult.Healthy(
                    "The deliveries outbox is processing within the expected interval.",
                    data);
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "The deliveries outbox state could not be read.",
                exception);
        }
    }
}
