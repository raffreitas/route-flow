using NSubstitute;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Deliveries.ConfirmAddressChangeInTransit;
using RouteFlow.Deliveries.Application.Exceptions;
using RouteFlow.Deliveries.Application.UnitTests.TestSupport;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Application.UnitTests.Deliveries;

public sealed class ConfirmAddressChangeInTransitCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenDeliveryIsInTransit_ShouldUpdateAddressAndPersistChanges()
    {
        // Arrange
        var delivery = DeliveryTestData.CreateDeliveryInTransit();
        var newAddress = CreateNewAddress();
        var repository = Substitute.For<IDeliveryRepository>();
        repository.GetByIdAsync(delivery.Id, CancellationToken.None).Returns(delivery);
        var handler = new ConfirmAddressChangeInTransitCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));

        // Act
        await handler.HandleAsync(
            new ConfirmAddressChangeInTransitCommand(delivery.Id, newAddress),
            CancellationToken.None);

        // Assert
        Assert.Equal(newAddress, delivery.Address);
        Assert.Equal(DeliveryStatus.InTransit, delivery.Status);
        Assert.Equal(DeliveryTestData.Now, delivery.UpdatedAt);
        await repository.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenDeliveryDoesNotExist_ShouldThrowAndNotPersist()
    {
        // Arrange
        var repository = Substitute.For<IDeliveryRepository>();
        var deliveryId = DeliveryId.New();
        var handler = new ConfirmAddressChangeInTransitCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));

        // Act
        var exception = await Assert.ThrowsAsync<DeliveryNotFoundException>(() =>
            handler.HandleAsync(
                new ConfirmAddressChangeInTransitCommand(deliveryId, CreateNewAddress()),
                CancellationToken.None));

        // Assert
        Assert.Equal(deliveryId, exception.DeliveryId);
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainRejectsTransition_ShouldThrowAndNotPersist()
    {
        // Arrange
        var delivery = DeliveryTestData.CreateRequestedDelivery();
        var repository = Substitute.For<IDeliveryRepository>();
        repository.GetByIdAsync(delivery.Id, CancellationToken.None).Returns(delivery);
        var handler = new ConfirmAddressChangeInTransitCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(
            new ConfirmAddressChangeInTransitCommand(delivery.Id, CreateNewAddress()),
            CancellationToken.None));
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static DeliveryAddress CreateNewAddress() =>
        new("Rua Nova", "42", null, "Centro", "São Paulo", "SP", "01000-000");
}
