using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.UnitTests;

public sealed class GuidV7Tests
{
    [Fact]
    public void StronglyTypedIds_ShouldGenerateVersion7Guids()
    {
        // Act
        var deliveryId = DeliveryId.New();
        var merchantId = MerchantId.New();
        var driverId = DriverId.New();

        // Assert - UUIDv7 has version '7' at index 14 in hyphenated format: xxxxxxxx-xxxx-7xxx-yxxx-xxxxxxxxxxxx
        Assert.Equal('7', deliveryId.Value.ToString()[14]);
        Assert.Equal('7', merchantId.Value.ToString()[14]);
        Assert.Equal('7', driverId.Value.ToString()[14]);
    }

    [Fact]
    public void StronglyTypedIds_ShouldBeMonotonicallyOrderedByTime()
    {
        // Act
        var first = DeliveryId.New();
        Thread.Sleep(5); // Ensure timestamp advance
        var second = DeliveryId.New();

        // Assert - UUIDv7 is sortable by creation time
        Assert.True(string.CompareOrdinal(first.Value.ToString(), second.Value.ToString()) < 0);
    }
}