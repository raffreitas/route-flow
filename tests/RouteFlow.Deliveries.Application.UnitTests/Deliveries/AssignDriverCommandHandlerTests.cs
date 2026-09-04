using NSubstitute;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Abstractions.Integrations;
using RouteFlow.Deliveries.Application.Deliveries.AssignDriver;
using RouteFlow.Deliveries.Application.Exceptions;
using RouteFlow.Deliveries.Application.UnitTests.TestSupport;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Application.UnitTests.Deliveries;

public sealed class AssignDriverCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenDeliveryExists_ShouldAssignDriverAndPersistChanges()
    {
        // Arrange
        var delivery = DeliveryTestData.CreateRequestedDelivery();
        var repository = Substitute.For<IDeliveryRepository>();
        repository.GetByIdAsync(delivery.Id, Arg.Any<CancellationToken>()).Returns(delivery);
        var driverId = DriverId.New();
        var handler = new AssignDriverCommandHandler(
            repository,
            CreateAvailableDriverGateway(driverId),
            new FixedTimeProvider(DeliveryTestData.Now));

        var command = new AssignDriverCommand(delivery.Id, driverId);

        // Act
        await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(DeliveryStatus.DriverAssigned, delivery.Status);
        Assert.Equal(driverId, delivery.AssignedDriverId);
        Assert.Equal(DeliveryTestData.Now, delivery.UpdatedAt);
        await repository.Received(1).GetByIdAsync(delivery.Id, CancellationToken.None);
        await repository.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenDeliveryDoesNotExist_ShouldThrowAndNotPersist()
    {
        // Arrange
        var repository = Substitute.For<IDeliveryRepository>();
        var missingId = DeliveryId.New();
        var handler = new AssignDriverCommandHandler(
            repository,
            Substitute.For<IDriverAvailabilityGateway>(),
            new FixedTimeProvider(DeliveryTestData.Now));

        var command = new AssignDriverCommand(missingId, DriverId.New());

        // Act
        var exception = await Assert.ThrowsAsync<DeliveryNotFoundException>(() =>
            handler.HandleAsync(command, CancellationToken.None));

        // Assert
        Assert.Equal(missingId, exception.DeliveryId);
        await repository.Received(1).GetByIdAsync(missingId, CancellationToken.None);
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainRejectsTransition_ShouldThrowAndNotPersist()
    {
        // Arrange
        var delivery = DeliveryTestData.CreateRequestedDelivery();
        delivery.AssignDriver(DriverId.New());
        var repository = Substitute.For<IDeliveryRepository>();
        repository.GetByIdAsync(delivery.Id, CancellationToken.None).Returns(delivery);
        var driverId = DriverId.New();
        var handler = new AssignDriverCommandHandler(
            repository,
            CreateAvailableDriverGateway(driverId),
            new FixedTimeProvider(DeliveryTestData.Now));
        var command = new AssignDriverCommand(delivery.Id, driverId);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command, CancellationToken.None));
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDeliveryRequiresVehicleType_ShouldAssignCompatibleVehicle()
    {
        // Arrange
        var delivery = DeliveryTestData.CreateDeliveryAtPickup(out _);
        delivery.ReportIncompatibleVehicle(
            VehicleType.Van,
            "Package requires a van",
            DeliveryTestData.Now.AddMinutes(-30));
        var repository = Substitute.For<IDeliveryRepository>();
        repository.GetByIdAsync(delivery.Id, CancellationToken.None).Returns(delivery);
        var driverId = DriverId.New();
        var handler = new AssignDriverCommandHandler(
            repository,
            CreateAvailableDriverGateway(driverId, VehicleType.Van),
            new FixedTimeProvider(DeliveryTestData.Now));
        var command = new AssignDriverCommand(delivery.Id, driverId);

        // Act
        await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(DeliveryStatus.DriverAssigned, delivery.Status);
        Assert.Equal(driverId, delivery.AssignedDriverId);
        await repository.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenDriverIsUnavailable_ShouldThrowAndNotPersist()
    {
        // Arrange
        var delivery = DeliveryTestData.CreateRequestedDelivery();
        var repository = Substitute.For<IDeliveryRepository>();
        repository.GetByIdAsync(delivery.Id, CancellationToken.None).Returns(delivery);
        var driverId = DriverId.New();
        var gateway = Substitute.For<IDriverAvailabilityGateway>();
        gateway.GetDriverAvailabilityAsync(driverId, CancellationToken.None)
            .Returns(new DriverAvailabilitySnapshot(driverId, false, VehicleType.Motorcycle));
        var handler = new AssignDriverCommandHandler(
            repository,
            gateway,
            new FixedTimeProvider(DeliveryTestData.Now));

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new AssignDriverCommand(delivery.Id, driverId), CancellationToken.None));
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static IDriverAvailabilityGateway CreateAvailableDriverGateway(
        DriverId driverId,
        VehicleType vehicleType = VehicleType.Motorcycle)
    {
        var gateway = Substitute.For<IDriverAvailabilityGateway>();
        gateway.GetDriverAvailabilityAsync(driverId, Arg.Any<CancellationToken>())
            .Returns(new DriverAvailabilitySnapshot(driverId, true, vehicleType));
        return gateway;
    }
}
