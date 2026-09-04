using RouteFlow.Fleet.Application.Abstractions;
using RouteFlow.Fleet.Contracts.DriverAvailability;
using RouteFlow.Fleet.Domain.Enums;
using RouteFlow.Fleet.Domain.ValueObjects;

namespace RouteFlow.Fleet.Application.Features.GetDriverAvailability;

public sealed class GetDriverAvailabilityQueryHandler(IDriverRepository repository) : IDriverAvailabilityReader
{
    public async Task<DriverAvailabilityResult?> GetAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        if (driverId == Guid.Empty)
        {
            return null;
        }

        var driver = await repository.GetByIdAsync(DriverId.From(driverId), cancellationToken);
        return driver is null
            ? null
            : new DriverAvailabilityResult(driver.Id.Value, driver.IsAvailable, Map(driver.VehicleType));
    }

    private static DriverVehicleType Map(VehicleType vehicleType) => vehicleType switch
    {
        VehicleType.Motorcycle => DriverVehicleType.Motorcycle,
        VehicleType.Car => DriverVehicleType.Car,
        VehicleType.Van => DriverVehicleType.Van,
        VehicleType.LightTruck => DriverVehicleType.LightTruck,
        _ => throw new ArgumentOutOfRangeException(nameof(vehicleType), vehicleType, null)
    };
}
