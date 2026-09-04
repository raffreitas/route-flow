using RouteFlow.Deliveries.Application.Abstractions.Integrations;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.Fleet.Contracts.DriverAvailability;

namespace RouteFlow.Deliveries.Infrastructure.Integrations;

internal sealed class FleetDriverAvailabilityGateway(IDriverAvailabilityReader reader)
    : IDriverAvailabilityGateway
{
    public async Task<DriverAvailabilitySnapshot?> GetDriverAvailabilityAsync(
        DriverId driverId,
        CancellationToken cancellationToken = default)
    {
        var result = await reader.GetAsync(driverId.Value, cancellationToken);
        return result is null
            ? null
            : new DriverAvailabilitySnapshot(driverId, result.IsAvailable, Map(result.VehicleType));
    }

    private static VehicleType Map(DriverVehicleType vehicleType) => vehicleType switch
    {
        DriverVehicleType.Motorcycle => VehicleType.Motorcycle,
        DriverVehicleType.Car => VehicleType.Car,
        DriverVehicleType.Van => VehicleType.Van,
        DriverVehicleType.LightTruck => VehicleType.LightTruck,
        _ => throw new ArgumentOutOfRangeException(nameof(vehicleType), vehicleType, null)
    };
}
