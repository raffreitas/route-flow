using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Abstractions.Integrations;

public interface IDriverAvailabilityGateway
{
    Task<DriverAvailabilitySnapshot?> GetDriverAvailabilityAsync(
        DriverId driverId,
        CancellationToken cancellationToken = default);
}

public sealed record DriverAvailabilitySnapshot(
    DriverId DriverId,
    bool IsAvailable,
    VehicleType VehicleType);
