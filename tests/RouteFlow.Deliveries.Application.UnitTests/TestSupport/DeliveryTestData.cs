using RouteFlow.Deliveries.Domain;
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
}
