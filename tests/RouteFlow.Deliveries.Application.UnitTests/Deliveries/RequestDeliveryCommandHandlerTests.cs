using NSubstitute;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Deliveries.RequestDelivery;
using RouteFlow.Deliveries.Application.UnitTests.TestSupport;
using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.UnitTests.Deliveries;

public sealed class RequestDeliveryCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCommandIsValid_ShouldCreateAndPersistRequestedDelivery()
    {
        // Arrange
        var repository = Substitute.For<IDeliveryRepository>();
        Delivery? addedDelivery = null;
        repository
            .AddAsync(Arg.Do<Delivery>(delivery => addedDelivery = delivery), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var merchantId = Guid.CreateVersion7();
        var command = new RequestDeliveryCommand(
            merchantId,
            new DeliveryAddressInput(
                "Av. Paulista",
                "1000",
                null,
                "Bela Vista",
                "São Paulo",
                "SP",
                "01310-100"),
            new PackageInput(1.0m, 10, 20, 30, "Envelope"));
        var handler = new RequestDeliveryCommandHandler(
            repository,
            new FixedTimeProvider(DeliveryTestData.Now));

        // Act
        var deliveryId = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        var delivery = Assert.IsType<Delivery>(addedDelivery);
        Assert.Equal(deliveryId, delivery.Id);
        Assert.Equal(DeliveryStatus.Requested, delivery.Status);
        Assert.Equal(MerchantId.From(merchantId), delivery.MerchantId);
        Assert.Equal("Av. Paulista", delivery.Address.Street);
        Assert.Equal("01310-100", delivery.Address.ZipCode);
        Assert.Equal(1.0m, delivery.Package.WeightKg);
        Assert.Equal(new PackageDimensions(10, 20, 30), delivery.Package.Dimensions);
        Assert.Equal("Envelope", delivery.Package.Description);
        Assert.Equal(DeliveryTestData.Now, delivery.CreatedAt);
        Assert.Equal(7, deliveryId.Value.Version);
        await repository.Received(1).AddAsync(delivery, CancellationToken.None);
        await repository.Received(1).SaveChangesAsync(CancellationToken.None);
    }
}
