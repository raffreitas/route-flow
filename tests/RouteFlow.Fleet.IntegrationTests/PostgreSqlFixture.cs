using Microsoft.EntityFrameworkCore;
using RouteFlow.Fleet.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace RouteFlow.Fleet.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18-alpine").Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public FleetDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FleetDbContext>()
            .UseNpgsql(
                ConnectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "fleet"))
            .Options;
        return new FleetDbContext(options);
    }
}
