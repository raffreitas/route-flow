namespace RouteFlow.Fleet.Contracts.DriverAvailability;

public sealed record DriverAvailabilityResult(
    Guid DriverId,
    bool IsAvailable,
    DriverVehicleType VehicleType);
