using Microsoft.EntityFrameworkCore;
using RouteFlow.Fleet.Domain;

namespace RouteFlow.Fleet.Infrastructure.Persistence;

public sealed class FleetDbContext(DbContextOptions<FleetDbContext> options) : DbContext(options)
{
    public DbSet<Driver> Drivers => Set<Driver>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("fleet");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FleetDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Driver>().Where(entry => entry.State == EntityState.Modified))
        {
            entry.Property(driver => driver.Version).CurrentValue++;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
