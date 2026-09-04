using NSubstitute;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Deliveries.ExpireOperationalIssue;
using RouteFlow.Deliveries.Application.Exceptions;
using RouteFlow.Deliveries.Application.UnitTests.TestSupport;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Application.UnitTests.Deliveries;

public sealed class ExpireOperationalIssueCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenResolutionWindowElapsed_ShouldInitiateReturnAndPersistChanges()
    {
        // Arrange
        var delivery = DeliveryTestData.CreateDeliveryInOperationalIssue(
            DeliveryTestData.Now.AddHours(-48));
        var repository = Substitute.For<IDeliveryRepository>();
        repository.GetByIdAsync(delivery.Id, CancellationToken.None).Returns(delivery);
        var handler = new ExpireOperationalIssueCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));

        // Act
        await handler.HandleAsync(
            new ExpireOperationalIssueCommand(delivery.Id),
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
        var handler = new ExpireOperationalIssueCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));

        // Act
        var exception = await Assert.ThrowsAsync<DeliveryNotFoundException>(() =>
            handler.HandleAsync(
                new ExpireOperationalIssueCommand(deliveryId),
                CancellationToken.None));

        // Assert
        Assert.Equal(deliveryId, exception.DeliveryId);
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BeforeResolutionWindowElapsed_ShouldThrowAndNotPersist()
    {
        // Arrange
        var delivery = DeliveryTestData.CreateDeliveryInOperationalIssue();
        var repository = Substitute.For<IDeliveryRepository>();
        repository.GetByIdAsync(delivery.Id, CancellationToken.None).Returns(delivery);
        var handler = new ExpireOperationalIssueCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(
            new ExpireOperationalIssueCommand(delivery.Id),
            CancellationToken.None));
        Assert.Equal(DeliveryStatus.InOperationalIssue, delivery.Status);
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
