using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.Events;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.UnitTests;

public sealed class DeliveryRecoveryTransitionsTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 3, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void UpdateAddressBeforePickup_WhenPackageIsAtMerchant_ShouldUpdateAddress()
    {
        // Arrange
        var delivery = CreateRequestedDelivery();
        var newAddress = new DeliveryAddress(
            "Rua Nova", "50", null, "Centro", "São Paulo", "SP", "01000-001");

        // Act
        delivery.UpdateAddressBeforePickup(newAddress, Now);

        // Assert
        Assert.Equal(newAddress, delivery.Address);
        Assert.Equal(DeliveryStatus.Requested, delivery.Status);
        var domainEvent = Assert.IsType<DeliveryAddressUpdatedDomainEvent>(delivery.DomainEvents.Last());
        Assert.Equal(newAddress, domainEvent.Address);
    }

    [Fact]
    public void ConfirmAddressChangeInTransit_WhenChangeWasApproved_ShouldUpdateAddressAndKeepDriverCustody()
    {
        // Arrange
        var delivery = CreateDeliveryInTransit();
        var newAddress = new DeliveryAddress(
            "Av. Nova", "100", "Bloco B", "Centro", "São Paulo", "SP", "01000-002");

        // Act
        delivery.ConfirmAddressChangeInTransit(newAddress, Now);

        // Assert
        Assert.Equal(newAddress, delivery.Address);
        Assert.Equal(DeliveryStatus.InTransit, delivery.Status);
        Assert.Equal(Custody.Driver, delivery.CurrentCustody);
        Assert.IsType<DeliveryAddressUpdatedDomainEvent>(delivery.DomainEvents.Last());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReleaseDriverBeforePickup_WhenDriverCannotContinue_ShouldReturnToRequestedAndReleaseDriver(
        bool alreadyDispatched)
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
        delivery.ReleaseDriverBeforePickup("Driver unavailable", Now);

        // Assert
        Assert.Equal(DeliveryStatus.Requested, delivery.Status);
        Assert.Null(delivery.AssignedDriverId);
        Assert.Equal(Custody.Merchant, delivery.CurrentCustody);
        var domainEvent = Assert.IsType<DriverReleasedDomainEvent>(delivery.DomainEvents.Last());
        Assert.Equal(driverId, domainEvent.DriverId);
        Assert.Equal("Driver unavailable", domainEvent.Reason);
    }

    [Fact]
    public void ReportIncompatibleVehicle_WhenDriverIsAtPickup_ShouldReturnToRequestedAndReleaseDriver()
    {
        // Arrange
        var delivery = CreateDeliveryAtPickup(out var driverId);

        // Act
        delivery.ReportIncompatibleVehicle(VehicleType.Van, "Package requires a van", Now);

        // Assert
        Assert.Equal(DeliveryStatus.Requested, delivery.Status);
        Assert.Null(delivery.AssignedDriverId);
        Assert.Equal(Custody.Merchant, delivery.CurrentCustody);
        Assert.Equal(VehicleType.Van, delivery.RequiredVehicleType);
        var domainEvent = Assert.IsType<IncompatibleVehicleReportedDomainEvent>(delivery.DomainEvents.Last());
        Assert.Equal(driverId, domainEvent.DriverId);
        Assert.Equal(VehicleType.Van, domainEvent.RequiredVehicleType);
    }

    [Fact]
    public void AssignDriver_AfterIncompatibleVehicleReported_ShouldRequireCompatibleVehicle()
    {
        // Arrange
        var delivery = CreateDeliveryAtPickup(out _);
        delivery.ReportIncompatibleVehicle(VehicleType.Van, "Package requires a van", Now);

        // Act & Assert
        Assert.Throws<DomainException>(() => delivery.AssignDriver(DriverId.New()));
        Assert.Throws<DomainException>(() => delivery.AssignDriver(DriverId.New(), VehicleType.Motorcycle));

        var compatibleDriverId = DriverId.New();
        delivery.AssignDriver(compatibleDriverId, VehicleType.Van);
        Assert.Equal(compatibleDriverId, delivery.AssignedDriverId);
    }

    [Fact]
    public void ReportIncompatibleVehicle_WhenRequiredVehicleTypeIsInvalid_ShouldThrowDomainException()
    {
        // Arrange
        var delivery = CreateDeliveryAtPickup(out var driverId);

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            delivery.ReportIncompatibleVehicle((VehicleType)999, "Unsupported vehicle", Now));
        Assert.Equal(DeliveryStatus.ArrivedAtPickup, delivery.Status);
        Assert.Equal(driverId, delivery.AssignedDriverId);
        Assert.Null(delivery.RequiredVehicleType);
    }

    [Fact]
    public void ResolveAddressIssue_WhenAddressWasCorrected_ShouldUpdateAddressAndWaitForNewRoute()
    {
        // Arrange
        var delivery = CreateDeliveryInOperationalIssue(Now.AddHours(-1));
        var correctedAddress = new DeliveryAddress(
            "Rua Corrigida", "42", "Sala 2", "Centro", "São Paulo", "SP", "01000-000");

        // Act
        delivery.ResolveAddressIssue(correctedAddress, Now);

        // Assert
        Assert.Equal(correctedAddress, delivery.Address);
        Assert.Equal(DeliveryStatus.PendingReschedule, delivery.Status);
        Assert.Equal(Custody.Driver, delivery.CurrentCustody);
        var domainEvent = Assert.IsType<DeliveryAddressUpdatedDomainEvent>(delivery.DomainEvents.Last());
        Assert.Equal(correctedAddress, domainEvent.Address);
    }

    [Fact]
    public void DispatchToNewRoute_AfterAddressIssueWasResolved_ShouldResumeTransit()
    {
        // Arrange
        var delivery = CreateDeliveryInOperationalIssue(Now.AddHours(-1));
        var correctedAddress = new DeliveryAddress(
            "Rua Corrigida", "42", null, "Centro", "São Paulo", "SP", "01000-000");
        delivery.ResolveAddressIssue(correctedAddress, Now);

        // Act
        delivery.DispatchToNewRoute(dispatchedAt: Now.AddMinutes(1));

        // Assert
        Assert.Equal(DeliveryStatus.InTransit, delivery.Status);
    }

    [Fact]
    public void ExpireOperationalIssue_WhenResolutionWindowElapsed_ShouldInitiateReturn()
    {
        // Arrange
        var issueStartedAt = Now.AddHours(-48);
        var delivery = CreateDeliveryInOperationalIssue(issueStartedAt);

        // Act
        delivery.ExpireOperationalIssue(Now);

        // Assert
        Assert.Equal(DeliveryStatus.InReturn, delivery.Status);
        Assert.IsType<OperationalIssueExpiredDomainEvent>(delivery.DomainEvents.ElementAt(delivery.DomainEvents.Count - 2));
        Assert.IsType<DeliveryReturnInitiatedDomainEvent>(delivery.DomainEvents.Last());
    }

    [Fact]
    public void ExpireOperationalIssue_BeforeResolutionWindowElapsed_ShouldThrowDomainException()
    {
        // Arrange
        var delivery = CreateDeliveryInOperationalIssue(Now.AddHours(-47));

        // Act & Assert
        Assert.Throws<DomainException>(() => delivery.ExpireOperationalIssue(Now));
        Assert.Equal(DeliveryStatus.InOperationalIssue, delivery.Status);
    }

    [Fact]
    public void AuthorizeReturn_WhenPackageIsAtHub_ShouldInitiateReturnAndKeepHubCustody()
    {
        // Arrange
        var delivery = CreateDeliveryAtHub();

        // Act
        delivery.AuthorizeReturn("Merchant authorized return", Now);

        // Assert
        Assert.Equal(DeliveryStatus.InReturn, delivery.Status);
        Assert.Equal(Custody.Hub, delivery.CurrentCustody);
        Assert.IsType<DeliveryReturnInitiatedDomainEvent>(delivery.DomainEvents.Last());
    }

    [Fact]
    public void DispatchToNewRoute_WhenPackageIsAtHub_ShouldAssignDriverAndTransferCustody()
    {
        // Arrange
        var delivery = CreateDeliveryAtHub();
        var newDriverId = DriverId.New();

        // Act
        delivery.DispatchToNewRoute(newDriverId, Now);

        // Assert
        Assert.Equal(DeliveryStatus.InTransit, delivery.Status);
        Assert.Equal(newDriverId, delivery.AssignedDriverId);
        Assert.Equal(Custody.Driver, delivery.CurrentCustody);
    }

    [Fact]
    public void DispatchToNewRoute_WhenPackageIsAtHubWithoutDriver_ShouldThrowDomainException()
    {
        // Arrange
        var delivery = CreateDeliveryAtHub();

        // Act & Assert
        Assert.Throws<DomainException>(() => delivery.DispatchToNewRoute(dispatchedAt: Now));
        Assert.Equal(DeliveryStatus.ReceivedAtHub, delivery.Status);
        Assert.Equal(Custody.Hub, delivery.CurrentCustody);
    }

    private static Delivery CreateRequestedDelivery() =>
        Delivery.Request(
            DeliveryId.New(),
            MerchantId.New(),
            new DeliveryAddress("Rua A", "1", null, "Centro", "São Paulo", "SP", "01000-000"),
            new PackageInfo(1, new PackageDimensions(10, 10, 10), "Package"));

    private static Delivery CreateDeliveryAtPickup(out DriverId driverId)
    {
        var delivery = CreateRequestedDelivery();
        driverId = DriverId.New();
        delivery.AssignDriver(driverId);
        delivery.StartDispatchToPickup();
        delivery.ConfirmArrivalAtPickup();
        return delivery;
    }

    private static Delivery CreateDeliveryInOperationalIssue(DateTimeOffset issueStartedAt)
    {
        var delivery = CreateDeliveryInTransit();
        delivery.RecordFailedAttempt(
            new FailureReason(FailureCategory.AddressNotFound, "Address not found"),
            issueStartedAt);
        return delivery;
    }

    private static Delivery CreateDeliveryAtHub()
    {
        var delivery = CreateDeliveryInTransit();
        delivery.CheckInPackageAtHub(HubId.New());
        return delivery;
    }

    private static Delivery CreateDeliveryInTransit()
    {
        var delivery = CreateDeliveryAtPickup(out var driverId);
        delivery.ConfirmPickup(driverId);
        return delivery;
    }
}
