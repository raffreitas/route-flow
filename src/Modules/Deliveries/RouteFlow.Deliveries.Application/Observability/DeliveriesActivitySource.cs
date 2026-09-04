using System.Diagnostics;

namespace RouteFlow.Deliveries.Application.Observability;

public static class DeliveriesActivitySource
{
    private const string Name = "RouteFlow.Deliveries";

    public static readonly ActivitySource Instance = new(Name);
}