using NSubstitute;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Deliveries.StartDispatchToPickup;
using RouteFlow.Deliveries.Application.Exceptions;
using RouteFlow.Deliveries.Application.UnitTests.TestSupport;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Application.UnitTests.Deliveries;

public sealed class StartDispatchToPickupCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenDeliveryIsAssigned_ShouldStartDispatchAndPersistChanges()
    {
        // Arrange
        var delivery = DeliveryTestData.CreateRequestedDelivery();
        delivery.AssignDriver(DriverId.New());
        var repository = Substitute.For<IDeliveryRepository>();
        repository.GetByIdAsync(delivery.Id, Arg.Any<CancellationToken>()).Returns(delivery);
        var handler = new StartDispatchToPickupCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));

        var command = new StartDispatchToPickupCommand(delivery.Id);

        // Act
        await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(DeliveryStatus.DispatchedToPickup, delivery.Status);
        Assert.Equal(DeliveryTestData.Now, delivery.UpdatedAt);
        await repository.Received(1).GetByIdAsync(delivery.Id, CancellationToken.None);
        await repository.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenDeliveryDoesNotExist_ShouldThrowAndNotPersist()
    {
        // Arrange
        var repository = Substitute.For<IDeliveryRepository>();
        var deliveryId = DeliveryId.New();
        var handler = new StartDispatchToPickupCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));
        var command = new StartDispatchToPickupCommand(deliveryId);

        // Act
        var exception = await Assert.ThrowsAsync<DeliveryNotFoundException>(() =>
            handler.HandleAsync(command, CancellationToken.None));

        // Assert
        Assert.Equal(deliveryId, exception.DeliveryId);
        await repository.Received(1).GetByIdAsync(deliveryId, CancellationToken.None);
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainRejectsTransition_ShouldThrowAndNotPersist()
    {
        // Arrange
        var delivery = DeliveryTestData.CreateRequestedDelivery();
        var repository = Substitute.For<IDeliveryRepository>();
        repository.GetByIdAsync(delivery.Id, CancellationToken.None).Returns(delivery);
        var handler = new StartDispatchToPickupCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));
        var command = new StartDispatchToPickupCommand(delivery.Id);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command, CancellationToken.None));
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
