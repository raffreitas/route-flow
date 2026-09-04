namespace RouteFlow.Api.Modules.Deliveries;

public sealed record DispatchToNewRouteRequest([NotEmptyGuid] Guid? DriverId = null);
