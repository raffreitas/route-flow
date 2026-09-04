namespace RouteFlow.Fleet.Application.UnitTests;

internal sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}
