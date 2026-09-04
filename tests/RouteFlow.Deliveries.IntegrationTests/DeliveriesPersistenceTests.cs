using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;
using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.Deliveries.Infrastructure;

namespace RouteFlow.Deliveries.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class DeliveriesPersistenceTests(PostgreSqlFixture fixture)
{
    private static readonly DateTimeOffset Now = new(2026, 9, 4, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Migrations_WhenDatabaseStartsEmpty_ShouldApplyInitialDeliveriesMigration()
    {
        // Arrange
        await using var dbContext = fixture.CreateDbContext();

        // Act
        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();

        // Assert
        Assert.Contains(appliedMigrations, migration => migration.EndsWith("_InitialDeliveries"));
    }

    [Fact]
    public async Task Repository_WhenDeliveryHasAttempts_ShouldRoundTripCompleteAggregate()
    {
        // Arrange
        var delivery = CreateDeliveryWithFailedAttempt();
        await using (var serviceProvider = CreateServiceProvider())
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
            await repository.AddAsync(delivery);
            await repository.SaveChangesAsync();
        }

        // Act
        Delivery? reloaded;
        await using (var serviceProvider = CreateServiceProvider())
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
            reloaded = await repository.GetByIdAsync(delivery.Id);
        }

        // Assert
        Assert.NotNull(reloaded);
        Assert.Equal(delivery.Id, reloaded.Id);
        Assert.Equal(delivery.MerchantId, reloaded.MerchantId);
        Assert.Equal(delivery.Address, reloaded.Address);
        Assert.Equal(delivery.Package, reloaded.Package);
        Assert.Equal(DeliveryStatus.PendingReschedule, reloaded.Status);
        Assert.Equal(Custody.Driver, reloaded.CurrentCustody);
        Assert.Equal(delivery.AssignedDriverId, reloaded.AssignedDriverId);
        Assert.Equal(1u, reloaded.Version);
        Assert.Empty(reloaded.DomainEvents);

        var attempt = Assert.Single(reloaded.Attempts);
        Assert.Equal(1, attempt.AttemptNumber);
        Assert.Equal(FailureCategory.RecipientAbsent, attempt.Reason.Category);
        Assert.Equal("No answer at the door", attempt.Notes);
    }

    [Fact]
    public async Task Repository_WhenTwoWritersUpdateSameDelivery_ShouldThrowDeliveryConcurrencyException()
    {
        // Arrange
        var delivery = CreateRequestedDelivery();
        await using var serviceProvider = CreateServiceProvider();
        await using (var seedScope = serviceProvider.CreateAsyncScope())
        {
            var repository = seedScope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
            await repository.AddAsync(delivery);
            await repository.SaveChangesAsync();
        }

        await using var firstScope = serviceProvider.CreateAsyncScope();
        await using var secondScope = serviceProvider.CreateAsyncScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
        var firstWriter = await firstRepository.GetByIdAsync(delivery.Id);
        var secondWriter = await secondRepository.GetByIdAsync(delivery.Id);

        firstWriter!.UpdateAddressBeforePickup(CreateAddress("100"), Now);
        secondWriter!.UpdateAddressBeforePickup(CreateAddress("200"), Now);
        await firstRepository.SaveChangesAsync();

        // Act
        var exception = await Assert.ThrowsAsync<DeliveryConcurrencyException>(
            () => secondRepository.SaveChangesAsync());

        // Assert
        Assert.Equal(delivery.Id, exception.DeliveryId);

        await using var verificationScope = serviceProvider.CreateAsyncScope();
        var verificationRepository = verificationScope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
        var persisted = await verificationRepository.GetByIdAsync(delivery.Id);
        Assert.Equal("100", persisted!.Address.Number);
        Assert.Equal(2u, persisted.Version);
    }

    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddDeliveriesInfrastructure(fixture.ConnectionString);
        return services.BuildServiceProvider();
    }

    private static Delivery CreateDeliveryWithFailedAttempt()
    {
        var delivery = CreateRequestedDelivery();
        var driverId = DriverId.New();
        delivery.AssignDriver(driverId, Now.AddMinutes(1));
        delivery.StartDispatchToPickup(Now.AddMinutes(2));
        delivery.ConfirmArrivalAtPickup(Now.AddMinutes(3));
        delivery.ConfirmPickup(driverId, Now.AddMinutes(4));
        delivery.RecordFailedAttempt(
            new FailureReason(FailureCategory.RecipientAbsent, "Recipient absent"),
            Now.AddMinutes(5),
            "No answer at the door");
        return delivery;
    }

    private static Delivery CreateRequestedDelivery() =>
        Delivery.Request(
            DeliveryId.New(),
            MerchantId.New(),
            CreateAddress("10"),
            new PackageInfo(1.5m, new PackageDimensions(20, 15, 10), "Parcel"),
            Now);

    private static DeliveryAddress CreateAddress(string number) =>
        new("Rua A", number, null, "Centro", "São Paulo", "SP", "01000-000");
}
