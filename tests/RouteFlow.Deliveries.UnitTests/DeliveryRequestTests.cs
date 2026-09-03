using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.Events;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.UnitTests;

public sealed class DeliveryRequestTests
{
    [Fact]
    public void Request_WithValidData_ShouldInitializeDeliveryInRequestedStatusAndMerchantCustody()
    {
        // Arrange
        var deliveryId = DeliveryId.New();
        var merchantId = MerchantId.New();
        var address = new DeliveryAddress(
            Street: "Av. Paulista",
            Number: "1000",
            Complement: "Apto 42",
            Neighborhood: "Bela Vista",
            City: "São Paulo",
            State: "SP",
            ZipCode: "01310-100");

        var package = new PackageInfo(
            WeightKg: 2.5m,
            Dimensions: new PackageDimensions(LengthCm: 30, WidthCm: 20, HeightCm: 10),
            Description: "Livros de Engenharia de Software");

        // Act
        var delivery = Delivery.Request(deliveryId, merchantId, address, package);

        // Assert
        Assert.Equal(deliveryId, delivery.Id);
        Assert.Equal(merchantId, delivery.MerchantId);
        Assert.Equal(address, delivery.Address);
        Assert.Equal(package, delivery.Package);
        Assert.Equal(DeliveryStatus.Requested, delivery.Status);
        Assert.Equal(Custody.Merchant, delivery.CurrentCustody);
        Assert.Null(delivery.AssignedDriverId);
        Assert.Empty(delivery.Attempts);

        var domainEvent = Assert.Single(delivery.DomainEvents);
        var requestedEvent = Assert.IsType<DeliveryRequestedDomainEvent>(domainEvent);
        Assert.Equal(deliveryId, requestedEvent.DeliveryId);
        Assert.Equal(merchantId, requestedEvent.MerchantId);
    }
}