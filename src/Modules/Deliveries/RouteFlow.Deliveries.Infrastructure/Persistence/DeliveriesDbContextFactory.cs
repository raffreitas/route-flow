using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RouteFlow.Deliveries.Infrastructure.Persistence;

public sealed class DeliveriesDbContextFactory : IDesignTimeDbContextFactory<DeliveriesDbContext>
{
    public DeliveriesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__deliveries")
            ?? "Host=localhost;Database=deliveries;Username=postgres";
        var options = new DbContextOptionsBuilder<DeliveriesDbContext>()
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "deliveries"))
            .Options;

        return new DeliveriesDbContext(options);
    }
}
