using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RouteFlow.Fleet.Infrastructure.Persistence;

public sealed class FleetDbContextFactory : IDesignTimeDbContextFactory<FleetDbContext>
{
    public FleetDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__fleet")
            ?? "Host=localhost;Database=fleet;Username=postgres";
        var options = new DbContextOptionsBuilder<FleetDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "fleet"))
            .Options;
        return new FleetDbContext(options);
    }
}
