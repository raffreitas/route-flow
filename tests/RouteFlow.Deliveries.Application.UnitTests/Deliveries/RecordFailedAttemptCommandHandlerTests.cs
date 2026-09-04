using NSubstitute;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Deliveries.RecordFailedAttempt;
using RouteFlow.Deliveries.Application.Exceptions;
using RouteFlow.Deliveries.Application.UnitTests.TestSupport;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Application.UnitTests.Deliveries;

public sealed class RecordFailedAttemptCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenDeliveryIsInTransit_ShouldRecordAttemptAndPersistChanges()
    {
        // Arrange
        var delivery = DeliveryTestData.CreateDeliveryInTransit();
        var reason = new FailureReason(FailureCategory.RecipientAbsent, "Recipient absent");
        var repository = Substitute.For<IDeliveryRepository>();
        repository.GetByIdAsync(delivery.Id, CancellationToken.None).Returns(delivery);
        var handler = new RecordFailedAttemptCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));

        // Act
        await handler.HandleAsync(
            new RecordFailedAttemptCommand(delivery.Id, reason, "No answer at the door"),
            CancellationToken.None);

        // Assert
        var attempt = Assert.Single(delivery.Attempts);
        Assert.Equal(reason, attempt.Reason);
        Assert.Equal("No answer at the door", attempt.Notes);
        Assert.Equal(DeliveryTestData.Now, attempt.OccurredAt);
        Assert.Equal(DeliveryStatus.PendingReschedule, delivery.Status);
        await repository.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenDeliveryDoesNotExist_ShouldThrowAndNotPersist()
    {
        // Arrange
        var repository = Substitute.For<IDeliveryRepository>();
        var deliveryId = DeliveryId.New();
        var handler = new RecordFailedAttemptCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));
        var reason = new FailureReason(FailureCategory.RecipientAbsent, "Recipient absent");

        // Act
        var exception = await Assert.ThrowsAsync<DeliveryNotFoundException>(() =>
            handler.HandleAsync(
                new RecordFailedAttemptCommand(deliveryId, reason),
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
        var handler = new RecordFailedAttemptCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));
        var reason = new FailureReason(FailureCategory.RecipientAbsent, "Recipient absent");

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(
            new RecordFailedAttemptCommand(delivery.Id, reason),
            CancellationToken.None));
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
