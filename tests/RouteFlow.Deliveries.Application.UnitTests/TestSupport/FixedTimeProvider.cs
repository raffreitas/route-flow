namespace RouteFlow.Deliveries.Application.UnitTests.TestSupport;

internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
