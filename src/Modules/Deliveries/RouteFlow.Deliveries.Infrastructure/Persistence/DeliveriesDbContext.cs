using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Infrastructure.Persistence.Outbox;

namespace RouteFlow.Deliveries.Infrastructure.Persistence;

public sealed class DeliveriesDbContext(DbContextOptions<DeliveriesDbContext> options) : DbContext(options)
{
    private static readonly JsonSerializerOptions OutboxSerializerOptions = new(JsonSerializerDefaults.Web);

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
        var aggregates = ChangeTracker
            .Entries<Delivery>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Select(entry => entry.Entity)
            .Where(delivery => delivery.DomainEvents.Count > 0)
            .ToArray();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var eventType = domainEvent.GetType();
                OutboxMessages.Add(OutboxMessage.Create(
                    aggregate.Id.Value,
                    domainEvent.OccurredAt,
                    eventType.FullName ?? eventType.Name,
                    JsonSerializer.Serialize(domainEvent, eventType, OutboxSerializerOptions)));
            }

            aggregate.ClearDomainEvents();
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
