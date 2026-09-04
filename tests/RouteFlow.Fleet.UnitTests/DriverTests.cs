using RouteFlow.Fleet.Domain;
using RouteFlow.Fleet.Domain.Enums;
using RouteFlow.Fleet.Domain.ValueObjects;

namespace RouteFlow.Fleet.UnitTests;

public sealed class DriverTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 4, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Register_WhenDataIsValid_ShouldCreateUnavailableDriver()
    {
        // Act
        var driver = Driver.Register(DriverId.New(), " Ana Silva ", VehicleType.Motorcycle, Now);

        // Assert
        Assert.Equal("Ana Silva", driver.Name);
        Assert.Equal(VehicleType.Motorcycle, driver.VehicleType);
        Assert.False(driver.IsAvailable);
        Assert.Equal(Now, driver.RegisteredAt);
        Assert.Null(driver.UpdatedAt);
    }

    [Fact]
    public void SetAvailability_WhenValueChanges_ShouldUpdateAvailabilityAndTimestamp()
    {
        // Arrange
        var driver = Driver.Register(DriverId.New(), "Ana Silva", VehicleType.Car, Now);
        var changedAt = Now.AddMinutes(5);

        // Act
        driver.SetAvailability(true, changedAt);

        // Assert
        Assert.True(driver.IsAvailable);
        Assert.Equal(changedAt, driver.UpdatedAt);
    }

    [Fact]
    public void SetAvailability_WhenValueDoesNotChange_ShouldRemainIdempotent()
    {
        // Arrange
        var driver = Driver.Register(DriverId.New(), "Ana Silva", VehicleType.Motorcycle, Now);

        // Act
        driver.SetAvailability(false, Now.AddMinutes(5));

        // Assert
        Assert.False(driver.IsAvailable);
        Assert.Null(driver.UpdatedAt);
    }
}
