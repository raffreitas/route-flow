namespace RouteFlow.Api.Modules.Deliveries;

public sealed record AssignDriverRequest(Guid DriverId, string? VehicleType = null);
