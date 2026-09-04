using NSubstitute;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Deliveries.ConfirmReturnToSender;
using RouteFlow.Deliveries.Application.Exceptions;
using RouteFlow.Deliveries.Application.UnitTests.TestSupport;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Application.UnitTests.Deliveries;

public sealed class ConfirmReturnToSenderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenDeliveryIsReturning_ShouldCompleteReturnAndPersistChanges()
    {
        // Arrange
        var delivery = DeliveryTestData.CreateDeliveryInReturn();
        var repository = Substitute.For<IDeliveryRepository>();
        repository.GetByIdAsync(delivery.Id, CancellationToken.None).Returns(delivery);
        var handler = new ConfirmReturnToSenderCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));

        // Act
        await handler.HandleAsync(
            new ConfirmReturnToSenderCommand(delivery.Id),
            CancellationToken.None);

        // Assert
        Assert.Equal(DeliveryStatus.ReturnedToSender, delivery.Status);
        Assert.Equal(Custody.Merchant, delivery.CurrentCustody);
        Assert.Equal(DeliveryTestData.Now, delivery.UpdatedAt);
        await repository.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenDeliveryDoesNotExist_ShouldThrowAndNotPersist()
    {
        // Arrange
        var repository = Substitute.For<IDeliveryRepository>();
        var deliveryId = DeliveryId.New();
        var handler = new ConfirmReturnToSenderCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));

        // Act
        var exception = await Assert.ThrowsAsync<DeliveryNotFoundException>(() =>
            handler.HandleAsync(
                new ConfirmReturnToSenderCommand(deliveryId),
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
        var handler = new ConfirmReturnToSenderCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(
            new ConfirmReturnToSenderCommand(delivery.Id),
            CancellationToken.None));
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
