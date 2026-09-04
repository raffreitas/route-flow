using NSubstitute;
using RouteFlow.Fleet.Application.Abstractions;
using RouteFlow.Fleet.Application.Features.SetDriverAvailability;
using RouteFlow.Fleet.Domain;
using RouteFlow.Fleet.Domain.Enums;
using RouteFlow.Fleet.Domain.ValueObjects;

namespace RouteFlow.Fleet.Application.UnitTests;

public sealed class SetDriverAvailabilityCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenDriverExists_ShouldChangeAvailabilityAndPersistChanges()
    {
        // Arrange
        var driver = Driver.Register(DriverId.New(), "Ana Silva", VehicleType.Van);
        var repository = Substitute.For<IDriverRepository>();
        repository.GetByIdAsync(driver.Id, CancellationToken.None).Returns(driver);
        var now = new DateTimeOffset(2026, 9, 4, 12, 0, 0, TimeSpan.Zero);
        var handler = new SetDriverAvailabilityCommandHandler(repository, new TestTimeProvider(now));

        // Act
        await handler.HandleAsync(
            new SetDriverAvailabilityCommand(driver.Id, true),
            CancellationToken.None);

        // Assert
        Assert.True(driver.IsAvailable);
        Assert.Equal(now, driver.UpdatedAt);
        await repository.Received(1).SaveChangesAsync(CancellationToken.None);
    }
}
