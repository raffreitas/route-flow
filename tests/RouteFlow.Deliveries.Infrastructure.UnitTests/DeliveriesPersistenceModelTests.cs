using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.Entities;
using RouteFlow.Deliveries.Infrastructure.Persistence;

namespace RouteFlow.Deliveries.Infrastructure.UnitTests;

public sealed class DeliveriesPersistenceModelTests
{
    private const string ConnectionString = "Host=localhost;Database=deliveries;Username=postgres";

    [Fact]
    public void Model_ShouldMapDeliveryToIsolatedSchemaWithConcurrencyToken()
    {
        // Arrange
        using var dbContext = CreateDbContext();

        // Act
        var delivery = dbContext.Model.FindEntityType(typeof(Delivery));

        // Assert
        Assert.NotNull(delivery);
        Assert.Equal("deliveries", delivery.GetSchema());
        Assert.Equal("deliveries", delivery.GetTableName());
        Assert.True(delivery.FindProperty(nameof(Delivery.Version))?.IsConcurrencyToken);
        Assert.Null(delivery.FindNavigation(nameof(Delivery.DomainEvents)));
    }

    [Fact]
    public void Model_ShouldRequireAttemptsToBelongToDeliveryWithUniqueSequence()
    {
        // Arrange
        using var dbContext = CreateDbContext();

        // Act
        var attempt = dbContext.Model.FindEntityType(typeof(DeliveryAttempt));
        var foreignKey = Assert.Single(attempt!.GetForeignKeys());
        var sequenceIndex = Assert.Single(attempt.GetIndexes());

        // Assert
        Assert.True(foreignKey.IsRequired);
        Assert.Equal(typeof(Delivery), foreignKey.PrincipalEntityType.ClrType);
        Assert.True(sequenceIndex.IsUnique);
        Assert.Equal(
            ["DeliveryId", nameof(DeliveryAttempt.AttemptNumber)],
            sequenceIndex.Properties.Select(property => property.Name));
    }

    [Fact]
    public void Model_ShouldConstrainOutboxTraceContextToW3CLimits()
    {
        // Arrange
        using var dbContext = CreateDbContext();

        // Act
        var outbox = dbContext.Model.GetEntityTypes()
            .Single(entity => entity.ClrType.Name == "OutboxMessage");

        // Assert
        Assert.Equal(55, outbox.FindProperty("TraceParent")?.GetMaxLength());
        Assert.Equal(512, outbox.FindProperty("TraceState")?.GetMaxLength());
    }

    [Fact]
    public void AddDeliveriesInfrastructure_ShouldRegisterDbContextAndRepository()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDeliveriesInfrastructure(ConnectionString);
        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        // Assert
        Assert.NotNull(scope.ServiceProvider.GetService<DeliveriesDbContext>());
        Assert.NotNull(scope.ServiceProvider.GetService<IDeliveryRepository>());
    }

    private static DeliveriesDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DeliveriesDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new DeliveriesDbContext(options);
    }
}
