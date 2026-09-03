using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.Events;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.UnitTests;

public sealed class DeliveryAttemptsAndOutcomesTests
{
    private static Delivery CreateDeliveryInTransit(out DriverId driverId)
    {
        var delivery = Delivery.Request(
            id: DeliveryId.New(),
            merchantId: MerchantId.New(),
            address: new DeliveryAddress("Rua das Flores", "123", null, "Jardins", "São Paulo", "SP", "01400-000"),
            package: new PackageInfo(1.5m, new PackageDimensions(20, 15, 10), "Produto Frágil")
        );

        driverId = DriverId.New();
        delivery.AssignDriver(driverId);
        delivery.StartDispatchToPickup();
        delivery.ConfirmArrivalAtPickup();
        delivery.ConfirmPickup(driverId);

        return delivery;
    }

    [Fact]
    public void ConfirmDeliveryToRecipient_WhenInTransit_ShouldCompleteDelivery_AndTransferCustodyToRecipient()
    {
        // Arrange
        var delivery = CreateDeliveryInTransit(out _);

        // Act
        delivery.ConfirmDeliveryToRecipient();

        // Assert
        Assert.Equal(DeliveryStatus.Completed, delivery.Status);
        Assert.Equal(Custody.Recipient, delivery.CurrentCustody);

        var completedEvent = delivery.DomainEvents
            .OfType<DeliveryCompletedDomainEvent>()
            .SingleOrDefault();

        Assert.NotNull(completedEvent);
        Assert.Equal(delivery.Id, completedEvent.DeliveryId);
    }

    [Fact]
    public void ConfirmDeliveryToRecipient_WhenNotInTransit_ShouldThrowDomainException()
    {
        // Arrange (still in Requested status)
        var delivery = Delivery.Request(
            DeliveryId.New(),
            MerchantId.New(),
            new DeliveryAddress("Rua A", "1", null, "Bairro", "Cidade", "UF", "00000-000"),
            new PackageInfo(1.0m, new PackageDimensions(10, 10, 10), "Pacote")
        );

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => delivery.ConfirmDeliveryToRecipient());
        Assert.Contains("InTransit", exception.Message);
    }

    [Fact]
    public void RecordFailedAttempt_WhenRecipientAbsent_Attempt1And2_ShouldTransitionToPendingReschedule()
    {
        // Arrange
        var delivery = CreateDeliveryInTransit(out _);
        var reason = new FailureReason(FailureCategory.RecipientAbsent, "Ninguém atendeu ao interfone");

        // Act - Attempt 1
        delivery.RecordFailedAttempt(reason);

        // Assert - Attempt 1
        Assert.Equal(DeliveryStatus.PendingReschedule, delivery.Status);
        Assert.Single(delivery.Attempts);
        Assert.Equal(1, delivery.Attempts.First().AttemptNumber);
        Assert.Equal(reason, delivery.Attempts.First().Reason);

        // Act - Re-dispatch and Attempt 2
        delivery.DispatchToNewRoute();
        Assert.Equal(DeliveryStatus.InTransit, delivery.Status);

        delivery.RecordFailedAttempt(reason);

        // Assert - Attempt 2
        Assert.Equal(DeliveryStatus.PendingReschedule, delivery.Status);
        Assert.Equal(2, delivery.Attempts.Count);
        Assert.Equal(2, delivery.Attempts.Last().AttemptNumber);
    }

    [Fact]
    public void RecordFailedAttempt_WhenRecipientAbsent_Attempt3_ShouldTransitionToInReturn()
    {
        // Arrange
        var delivery = CreateDeliveryInTransit(out _);
        var reason = new FailureReason(FailureCategory.RecipientAbsent, "Destinatário ausente");

        // Attempt 1
        delivery.RecordFailedAttempt(reason);
        delivery.DispatchToNewRoute();

        // Attempt 2
        delivery.RecordFailedAttempt(reason);
        delivery.DispatchToNewRoute();

        // Act - Attempt 3 (Maximum reached)
        delivery.RecordFailedAttempt(reason);

        // Assert
        Assert.Equal(DeliveryStatus.InReturn, delivery.Status);
        Assert.Equal(3, delivery.Attempts.Count);

        var returnEvent = delivery.DomainEvents
            .OfType<DeliveryReturnInitiatedDomainEvent>()
            .SingleOrDefault();

        Assert.NotNull(returnEvent);
        Assert.Equal(delivery.Id, returnEvent.DeliveryId);
    }

    [Fact]
    public void RecordFailedAttempt_WhenRecipientRefused_ShouldTransitionDirectlyToInReturn()
    {
        // Arrange
        var delivery = CreateDeliveryInTransit(out _);
        var reason = new FailureReason(FailureCategory.RecipientRefused, "Destinatário alegou que cancelou o pedido");

        // Act
        delivery.RecordFailedAttempt(reason);

        // Assert
        Assert.Equal(DeliveryStatus.InReturn, delivery.Status);
        Assert.Single(delivery.Attempts);

        var returnEvent = delivery.DomainEvents
            .OfType<DeliveryReturnInitiatedDomainEvent>()
            .SingleOrDefault();

        Assert.NotNull(returnEvent);
        Assert.Equal(delivery.Id, returnEvent.DeliveryId);
    }

    [Fact]
    public void RecordFailedAttempt_WhenAddressNotFound_ShouldTransitionToInOperationalIssue()
    {
        // Arrange
        var delivery = CreateDeliveryInTransit(out _);
        var reason = new FailureReason(FailureCategory.AddressNotFound, "Número 999 não existe na rua");

        // Act
        delivery.RecordFailedAttempt(reason);

        // Assert
        Assert.Equal(DeliveryStatus.InOperationalIssue, delivery.Status);
        Assert.Single(delivery.Attempts);

        var issueEvent = delivery.DomainEvents
            .OfType<DeliverySentToOperationalIssueDomainEvent>()
            .SingleOrDefault();

        Assert.NotNull(issueEvent);
        Assert.Equal(delivery.Id, issueEvent.DeliveryId);
    }

    [Fact]
    public void ConfirmReturnToSender_WhenInReturn_ShouldTransitionToReturnedToSender_AndTransferCustodyToMerchant()
    {
        // Arrange
        var delivery = CreateDeliveryInTransit(out _);
        delivery.RecordFailedAttempt(new FailureReason(FailureCategory.RecipientRefused, "Recusado"));
        Assert.Equal(DeliveryStatus.InReturn, delivery.Status);

        // Act
        delivery.ConfirmReturnToSender();

        // Assert
        Assert.Equal(DeliveryStatus.ReturnedToSender, delivery.Status);
        Assert.Equal(Custody.Merchant, delivery.CurrentCustody);

        var returnedEvent = delivery.DomainEvents
            .OfType<DeliveryReturnedToSenderDomainEvent>()
            .SingleOrDefault();

        Assert.NotNull(returnedEvent);
        Assert.Equal(delivery.Id, returnedEvent.DeliveryId);
    }

    [Fact]
    public void CompletedDelivery_ShouldNotAllowFurtherStateTransitions()
    {
        // Arrange
        var delivery = CreateDeliveryInTransit(out var driverId);
        delivery.ConfirmDeliveryToRecipient();
        Assert.Equal(DeliveryStatus.Completed, delivery.Status);

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => delivery.AssignDriver(driverId));
        Assert.Contains("terminal state", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}