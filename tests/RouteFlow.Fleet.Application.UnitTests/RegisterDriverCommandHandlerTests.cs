using NSubstitute;
using RouteFlow.Fleet.Application.Abstractions;
using RouteFlow.Fleet.Application.Features.RegisterDriver;

namespace RouteFlow.Fleet.Application.UnitTests;

public sealed class RegisterDriverCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCommandIsValid_ShouldRegisterDriverAndPersistChanges()
    {
        // Arrange
        var repository = Substitute.For<IDriverRepository>();
        var now = new DateTimeOffset(2026, 9, 4, 12, 0, 0, TimeSpan.Zero);
        var handler = new RegisterDriverCommandHandler(repository, new TestTimeProvider(now));

        // Act
        var driverId = await handler.HandleAsync(
            new RegisterDriverCommand("Ana Silva", "motorcycle"),
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, driverId.Value);
        await repository.Received(1).AddAsync(
            Arg.Is<RouteFlow.Fleet.Domain.Driver>(driver =>
                driver.Id == driverId && driver.Name == "Ana Silva" && driver.RegisteredAt == now),
            CancellationToken.None);
        await repository.Received(1).SaveChangesAsync(CancellationToken.None);
    }
}
