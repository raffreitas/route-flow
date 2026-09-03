using NSubstitute;
using RouteFlow.Deliveries.Application.Abstractions;
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
        var handler = new AssignDriverCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));
        var command = new AssignDriverCommand(delivery.Id, DriverId.New());

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command, CancellationToken.None));
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
