namespace RouteFlow.Api.Modules.Deliveries;

public sealed record DriverActionRequest([NotEmptyGuid] Guid DriverId);
