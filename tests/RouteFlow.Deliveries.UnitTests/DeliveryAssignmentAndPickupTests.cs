using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.Events;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.UnitTests;

public sealed class DeliveryAssignmentAndPickupTests
{
    private static Delivery CreateRequestedDelivery()
    {
        return Delivery.Request(
            id: DeliveryId.New(),
            merchantId: MerchantId.New(),
            address: new DeliveryAddress("Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP", "01310-100"),
            package: new PackageInfo(1.0m, new PackageDimensions(10, 10, 10), "Envelope")
        );
    }

    [Fact]
    public void AssignDriver_WhenStatusIsRequested_ShouldTransitionToDriverAssigned_AndKeepMerchantCustody()
    {
        // Arrange
        var delivery = CreateRequestedDelivery();
        var driverId = DriverId.New();

        // Act
        delivery.AssignDriver(driverId);

        // Assert
        Assert.Equal(DeliveryStatus.DriverAssigned, delivery.Status);
        Assert.Equal(driverId, delivery.AssignedDriverId);
        Assert.Equal(Custody.Merchant, delivery.CurrentCustody);

        var driverAssignedEvent = delivery.DomainEvents
            .OfType<DriverAssignedDomainEvent>()
            .SingleOrDefault();

        Assert.NotNull(driverAssignedEvent);
        Assert.Equal(delivery.Id, driverAssignedEvent.DeliveryId);
        Assert.Equal(driverId, driverAssignedEvent.DriverId);
    }

    [Fact]
    public void AssignDriver_WhenAlreadyAssigned_ShouldThrowDomainException()
    {
        // Arrange
        var delivery = CreateRequestedDelivery();
        var driverA = DriverId.New();
        var driverB = DriverId.New();
        delivery.AssignDriver(driverA);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => delivery.AssignDriver(driverB));
        Assert.Contains("Requested", exception.Message);
    }

    [Fact]
    public void ConfirmPickup_WhenDriverMatchesAndStatusIsArrivedAtPickup_ShouldTransitionToInTransit_AndTransferCustodyToDriver()
    {
        // Arrange
        var delivery = CreateRequestedDelivery();
        var driverId = DriverId.New();
        delivery.AssignDriver(driverId);
        delivery.StartDispatchToPickup();
        delivery.ConfirmArrivalAtPickup();

        Assert.Equal(DeliveryStatus.ArrivedAtPickup, delivery.Status);
        Assert.Equal(Custody.Merchant, delivery.CurrentCustody);

        // Act
        delivery.ConfirmPickup(driverId);

        // Assert
        Assert.Equal(DeliveryStatus.InTransit, delivery.Status);
        Assert.Equal(Custody.Driver, delivery.CurrentCustody);

        var pickupEvent = delivery.DomainEvents
            .OfType<PackagePickedUpDomainEvent>()
            .SingleOrDefault();

        Assert.NotNull(pickupEvent);
        Assert.Equal(delivery.Id, pickupEvent.DeliveryId);
        Assert.Equal(driverId, pickupEvent.DriverId);
    }

    [Fact]
    public void ConfirmPickup_WhenDifferentDriverTriesToPickUp_ShouldThrowDomainException()
    {
        // Arrange
        var delivery = CreateRequestedDelivery();
        var assignedDriver = DriverId.New();
        var impostorDriver = DriverId.New();

        delivery.AssignDriver(assignedDriver);
        delivery.StartDispatchToPickup();
        delivery.ConfirmArrivalAtPickup();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => delivery.ConfirmPickup(impostorDriver));
        Assert.Contains("not assigned", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConfirmPickup_BeforeArrivingAtPickup_ShouldThrowDomainException()
    {
        // Arrange
        var delivery = CreateRequestedDelivery();
        var driverId = DriverId.New();
        delivery.AssignDriver(driverId);

        // Act & Assert (still in DriverAssigned status, hasn't arrived at the store yet)
        var exception = Assert.Throws<DomainException>(() => delivery.ConfirmPickup(driverId));
        Assert.Contains("ArrivedAtPickup", exception.Message);
    }
}