using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.UnitTests.TestSupport;

internal static class DeliveryTestData
{
    internal static readonly DateTimeOffset Now = new(2026, 9, 3, 12, 0, 0, TimeSpan.Zero);

    internal static Delivery CreateRequestedDelivery() =>
        Delivery.Request(
            DeliveryId.New(),
            MerchantId.New(),
            CreateAddress(),
            CreatePackage(),
            Now.AddHours(-1));

    internal static DeliveryAddress CreateAddress() =>
        new("Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP", "01310-100");

    internal static PackageInfo CreatePackage() =>
        new(1.0m, new PackageDimensions(10, 10, 10), "Envelope");

    internal static Delivery CreateDeliveryAtPickup(out DriverId driverId)
    {
        var delivery = CreateRequestedDelivery();
        driverId = DriverId.New();
        delivery.AssignDriver(driverId, Now.AddMinutes(-45));
        delivery.StartDispatchToPickup(Now.AddMinutes(-40));
        delivery.ConfirmArrivalAtPickup(Now.AddMinutes(-35));
        return delivery;
    }

    internal static Delivery CreateDeliveryInOperationalIssue()
    {
        var delivery = CreateDeliveryAtPickup(out var driverId);
        delivery.ConfirmPickup(driverId, Now.AddMinutes(-30));
        delivery.RecordFailedAttempt(
            new FailureReason(FailureCategory.AddressNotFound, "Address not found"),
            Now.AddMinutes(-20));
        return delivery;
    }

    internal static Delivery CreateDeliveryPendingReschedule()
    {
        var delivery = CreateDeliveryAtPickup(out var driverId);
        delivery.ConfirmPickup(driverId, Now.AddMinutes(-30));
        delivery.RecordFailedAttempt(
            new FailureReason(FailureCategory.RecipientAbsent, "Recipient absent"),
            Now.AddMinutes(-20));
        return delivery;
    }
}
