using Microsoft.EntityFrameworkCore;
using RouteFlow.Deliveries.Domain;

namespace RouteFlow.Deliveries.Infrastructure.Persistence;

public sealed class DeliveriesDbContext(DbContextOptions<DeliveriesDbContext> options) : DbContext(options)
{
    public DbSet<Delivery> Deliveries => Set<Delivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("deliveries");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeliveriesDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        IncrementAggregateVersions();
        return await base.SaveChangesAsync(cancellationToken);
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
