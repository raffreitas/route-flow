using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Infrastructure.Messaging;
using RouteFlow.Deliveries.Infrastructure.Persistence.Outbox;

namespace RouteFlow.Deliveries.Infrastructure.Persistence;

public sealed class DeliveriesDbContext(DbContextOptions<DeliveriesDbContext> options) : DbContext(options)
{
    public DbSet<Delivery> Deliveries => Set<Delivery>();

    internal DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("deliveries");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeliveriesDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddOutboxMessages();
        IncrementAggregateVersions();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AddOutboxMessages()
    {
        var activity = Activity.Current;
        var traceParent = activity?.IdFormat == ActivityIdFormat.W3C ? activity.Id : null;
        var traceState = traceParent is null ? null : activity?.TraceStateString;
        var entries = ChangeTracker
            .Entries<Delivery>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .ToArray();

        foreach (var entry in entries)
        {
            foreach (var (type, occurredAt, integrationEvent) in DeliveryIntegrationEventMapper.Map(entry))
            {
                OutboxMessages.Add(OutboxMessage.Create(
                    integrationEvent.DeliveryId,
                    occurredAt,
                    type,
                    IntegrationEventSerializer.Serialize(integrationEvent),
                    traceParent,
                    traceState));
            }

            entry.Entity.ClearDomainEvents();
        }
    }

    private void IncrementAggregateVersions()
    {
        foreach (var entry in ChangeTracker.Entries<Delivery>().Where(entry => entry.State == EntityState.Modified))
        {
            var version = entry.Property(delivery => delivery.Version);
            version.CurrentValue++;
        }
    }
}
