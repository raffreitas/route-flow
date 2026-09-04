using NSubstitute;
using RouteFlow.Fleet.Application.Abstractions;
using RouteFlow.Fleet.Application.Features.GetDriverAvailability;
using RouteFlow.Fleet.Contracts.DriverAvailability;
using RouteFlow.Fleet.Domain;
using RouteFlow.Fleet.Domain.Enums;
using RouteFlow.Fleet.Domain.ValueObjects;

namespace RouteFlow.Fleet.Application.UnitTests;

public sealed class GetDriverAvailabilityQueryHandlerTests
{
    [Fact]
    public async Task Get_WhenDriverExists_ShouldReturnPublicAvailabilityContract()
    {
        // Arrange
        var driver = Driver.Register(DriverId.New(), "Ana Silva", VehicleType.LightTruck);
        driver.SetAvailability(true);
        var repository = Substitute.For<IDriverRepository>();
        repository.GetByIdAsync(driver.Id, CancellationToken.None).Returns(driver);
        var handler = new GetDriverAvailabilityQueryHandler(repository);

        // Act
        var result = await handler.GetAsync(driver.Id.Value, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(driver.Id.Value, result.DriverId);
        Assert.True(result.IsAvailable);
        Assert.Equal(DriverVehicleType.LightTruck, result.VehicleType);
    }
}
