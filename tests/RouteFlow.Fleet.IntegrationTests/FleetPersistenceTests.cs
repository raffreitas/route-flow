using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RouteFlow.Fleet.Application.Abstractions;
using RouteFlow.Fleet.Domain;
using RouteFlow.Fleet.Domain.Enums;
using RouteFlow.Fleet.Domain.ValueObjects;
using RouteFlow.Fleet.Infrastructure;

namespace RouteFlow.Fleet.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class FleetPersistenceTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task Migrations_WhenDatabaseStartsEmpty_ShouldApplyInitialFleetMigration()
    {
        await using var dbContext = fixture.CreateDbContext();

        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();

        Assert.Contains(appliedMigrations, migration => migration.EndsWith("_InitialFleet"));
    }

    [Fact]
    public async Task Repository_WhenDriverAvailabilityChanges_ShouldRoundTripAggregate()
    {
        // Arrange
        var registeredAt = new DateTimeOffset(2026, 9, 4, 12, 0, 0, TimeSpan.Zero);
        var updatedAt = registeredAt.AddMinutes(10);
        var driver = Driver.Register(DriverId.New(), "Ana Silva", VehicleType.Van, registeredAt);
        await using var serviceProvider = new ServiceCollection()
            .AddFleetInfrastructure(fixture.ConnectionString)
            .BuildServiceProvider();

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IDriverRepository>();
            await repository.AddAsync(driver);
            await repository.SaveChangesAsync();
        }

        // Act
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IDriverRepository>();
            var persisted = await repository.GetByIdAsync(driver.Id);
            Assert.NotNull(persisted);
            persisted.SetAvailability(true, updatedAt);
            await repository.SaveChangesAsync();
        }

        // Assert
        await using var assertionScope = serviceProvider.CreateAsyncScope();
        var assertionRepository = assertionScope.ServiceProvider.GetRequiredService<IDriverRepository>();
        var result = await assertionRepository.GetByIdAsync(driver.Id);
        Assert.NotNull(result);
        Assert.True(result.IsAvailable);
        Assert.Equal(updatedAt, result.UpdatedAt);
        Assert.Equal((uint)2, result.Version);
    }
}
