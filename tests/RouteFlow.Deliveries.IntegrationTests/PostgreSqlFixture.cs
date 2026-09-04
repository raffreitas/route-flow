using Microsoft.EntityFrameworkCore;
using RouteFlow.Deliveries.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace RouteFlow.Deliveries.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18-alpine")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    public DeliveriesDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DeliveriesDbContext>()
            .UseNpgsql(
                ConnectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "deliveries"))
            .Options;

        return new DeliveriesDbContext(options);
    }
}
