namespace RouteFlow.Deliveries.UnitTests;

using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.Events;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

public sealed class DeliveryCancellationAndIncidentsTests
{
    private static Delivery CreateRequestedDelivery()
    {
        return Delivery.Request(
            id: DeliveryId.New(),
            merchantId: MerchantId.New(),
            address: new DeliveryAddress("Av. Ipiranga", "200", null, "República", "São Paulo", "SP", "01046-010"),
            package: new PackageInfo(0.8m, new PackageDimensions(15, 10, 5), "Acessório eletrônico")
        );
    }

    private static Delivery CreateDeliveryInTransit(out DriverId driverId)
    {
        var delivery = CreateRequestedDelivery();
        driverId = DriverId.New();
        delivery.AssignDriver(driverId);
        delivery.StartDispatchToPickup();
        delivery.ConfirmArrivalAtPickup();
        delivery.ConfirmPickup(driverId);
        return delivery;
    }

    [Fact]
    public void Cancel_WhenBeforePickup_ShouldTransitionToCanceled_AndEmitDeliveryCanceledEvent()
    {
        // Arrange
        var delivery = CreateRequestedDelivery();
        var driverId = DriverId.New();
        delivery.AssignDriver(driverId);
        Assert.Equal(DeliveryStatus.DriverAssigned, delivery.Status);

        // Act
        delivery.Cancel("Lojista solicitou cancelamento antes do pacote sair da loja");

        // Assert
        Assert.Equal(DeliveryStatus.Canceled, delivery.Status);
        Assert.Equal(Custody.Merchant, delivery.CurrentCustody);
        Assert.Null(delivery.AssignedDriverId);

        var canceledEvent = delivery.DomainEvents
            .OfType<DeliveryCanceledDomainEvent>()
            .SingleOrDefault();

        Assert.NotNull(canceledEvent);
        Assert.Equal(delivery.Id, canceledEvent.DeliveryId);
    }

    [Fact]
    public void Cancel_WhenInTransit_ShouldTransitionToInReturn_AndRequireReturnToMerchant()
    {
        // Arrange
        var delivery = CreateDeliveryInTransit(out _);
        Assert.Equal(DeliveryStatus.InTransit, delivery.Status);
        Assert.Equal(Custody.Driver, delivery.CurrentCustody);

        // Act
        delivery.Cancel("Comprador desistiu com a entrega em rota");

        // Assert - Cannot be simply Canceled because package is with driver! Must initiate return.
        Assert.Equal(DeliveryStatus.InReturn, delivery.Status);
        Assert.Equal(Custody.Driver, delivery.CurrentCustody);

        var returnEvent = delivery.DomainEvents
            .OfType<DeliveryReturnInitiatedDomainEvent>()
            .SingleOrDefault();

        Assert.NotNull(returnEvent);
        Assert.Equal(delivery.Id, returnEvent.DeliveryId);
    }

    [Fact]
    public void Cancel_WhenDriverIsAtPickup_ShouldCancelReleaseDriverAndEmitPickupCanceledEvent()
    {
        // Arrange
        var delivery = CreateRequestedDelivery();
        var driverId = DriverId.New();
        delivery.AssignDriver(driverId);
        delivery.StartDispatchToPickup();
        delivery.ConfirmArrivalAtPickup();

        // Act
        delivery.Cancel("Merchant canceled at pickup");

        // Assert
        Assert.Equal(DeliveryStatus.Canceled, delivery.Status);
        Assert.Equal(Custody.Merchant, delivery.CurrentCustody);
        Assert.Null(delivery.AssignedDriverId);
        var domainEvent = Assert.IsType<PickupCanceledByMerchantDomainEvent>(delivery.DomainEvents.Last());
        Assert.Equal(driverId, domainEvent.DriverId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Cancel_WhenDriverIsAssignedBeforeArrival_ShouldEmitDriverReleasedEvent(bool alreadyDispatched)
    {
        // Arrange
        var delivery = CreateRequestedDelivery();
        var driverId = DriverId.New();
        delivery.AssignDriver(driverId);
        if (alreadyDispatched)
        {
            delivery.StartDispatchToPickup();
        }

        // Act
        delivery.Cancel("Merchant canceled before pickup");

        // Assert
        Assert.Null(delivery.AssignedDriverId);
        var releasedEvent = Assert.Single(delivery.DomainEvents.OfType<DriverReleasedDomainEvent>());
        Assert.Equal(driverId, releasedEvent.DriverId);
        Assert.IsType<DeliveryCanceledDomainEvent>(delivery.DomainEvents.Last());
    }

    [Fact]
    public void Cancel_WhenAlreadyCompleted_ShouldThrowDomainException()
    {
        // Arrange
        var delivery = CreateDeliveryInTransit(out _);
        delivery.ConfirmDeliveryToRecipient();
        Assert.Equal(DeliveryStatus.Completed, delivery.Status);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => delivery.Cancel("Tentativa de cancelar após entrega"));
        Assert.Contains("terminal state", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReportTransitIncident_WhenInTransit_ShouldTransitionToHeldDueToIncident_AndKeepDriverCustody()
    {
        // Arrange
        var delivery = CreateDeliveryInTransit(out var driverId);

        // Act
        delivery.ReportTransitIncident("Pane mecânica no veículo do motorista");

        // Assert
        Assert.Equal(DeliveryStatus.HeldDueToIncident, delivery.Status);
        Assert.Equal(Custody.Driver, delivery.CurrentCustody);

        var incidentEvent = delivery.DomainEvents
            .OfType<TransitIncidentReportedDomainEvent>()
            .SingleOrDefault();

        Assert.NotNull(incidentEvent);
        Assert.Equal(delivery.Id, incidentEvent.DeliveryId);
        Assert.Equal(driverId, incidentEvent.DriverId);
    }

    [Fact]
    public void CheckInPackageAtHub_WhenHeldDueToIncident_ShouldTransitionToReceivedAtHub_AndTransferCustodyToHub()
    {
        // Arrange
        var delivery = CreateDeliveryInTransit(out _);
        delivery.ReportTransitIncident("Veículo quebrado");
        Assert.Equal(DeliveryStatus.HeldDueToIncident, delivery.Status);

        var hubId = HubId.New();

        // Act
        delivery.CheckInPackageAtHub(hubId);

        // Assert - Physical custody is now safely with the RouteFlow Hub!
        Assert.Equal(DeliveryStatus.ReceivedAtHub, delivery.Status);
        Assert.Equal(Custody.Hub, delivery.CurrentCustody);
        Assert.Null(delivery.AssignedDriverId);

        var hubEvent = delivery.DomainEvents
            .OfType<PackageReceivedAtHubDomainEvent>()
            .SingleOrDefault();

        Assert.NotNull(hubEvent);
        Assert.Equal(delivery.Id, hubEvent.DeliveryId);
        Assert.Equal(hubId, hubEvent.HubId);
    }
}
