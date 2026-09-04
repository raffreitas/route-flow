using NSubstitute;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Deliveries.AuthorizeReturn;
using RouteFlow.Deliveries.Application.Exceptions;
using RouteFlow.Deliveries.Application.UnitTests.TestSupport;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Application.UnitTests.Deliveries;

public sealed class AuthorizeReturnCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenDeliveryHasOperationalIssue_ShouldInitiateReturnAndPersistChanges()
    {
        // Arrange
        var delivery = DeliveryTestData.CreateDeliveryInOperationalIssue();
        var repository = Substitute.For<IDeliveryRepository>();
        repository.GetByIdAsync(delivery.Id, CancellationToken.None).Returns(delivery);
        var handler = new AuthorizeReturnCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));

        // Act
        await handler.HandleAsync(
            new AuthorizeReturnCommand(delivery.Id, "Merchant authorized return"),
            CancellationToken.None);

        // Assert
        Assert.Equal(DeliveryStatus.InReturn, delivery.Status);
        Assert.Equal(DeliveryTestData.Now, delivery.UpdatedAt);
        await repository.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenDeliveryDoesNotExist_ShouldThrowAndNotPersist()
    {
        // Arrange
        var repository = Substitute.For<IDeliveryRepository>();
        var deliveryId = DeliveryId.New();
        var handler = new AuthorizeReturnCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));

        // Act
        var exception = await Assert.ThrowsAsync<DeliveryNotFoundException>(() =>
            handler.HandleAsync(
                new AuthorizeReturnCommand(deliveryId, "Merchant authorized return"),
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
        var handler = new AuthorizeReturnCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(
            new AuthorizeReturnCommand(delivery.Id, "Merchant authorized return"),
            CancellationToken.None));
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
