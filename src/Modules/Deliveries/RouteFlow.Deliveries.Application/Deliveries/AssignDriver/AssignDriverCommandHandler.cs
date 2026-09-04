using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Abstractions.Integrations;
using RouteFlow.Deliveries.Application.Exceptions;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Application.Deliveries.AssignDriver;

public sealed class AssignDriverCommandHandler(
    IDeliveryRepository repository,
    IDriverAvailabilityGateway driverAvailabilityGateway,
    TimeProvider timeProvider)
{
    public async Task HandleAsync(AssignDriverCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        var driver = await driverAvailabilityGateway.GetDriverAvailabilityAsync(
            command.DriverId,
            cancellationToken)
            ?? throw new DomainException($"Driver '{command.DriverId}' is not registered.");

        if (!driver.IsAvailable)
        {
            throw new DomainException($"Driver '{command.DriverId}' is not available.");
        }

        if (command.VehicleType is { } requestedVehicleType && requestedVehicleType != driver.VehicleType)
        {
            throw new DomainException(
                $"Driver '{command.DriverId}' uses vehicle type '{driver.VehicleType}', not '{requestedVehicleType}'.");
        }

        var assignedAt = timeProvider.GetUtcNow();
        delivery.AssignDriver(command.DriverId, driver.VehicleType, assignedAt);

        await repository.SaveChangesAsync(cancellationToken);
    }
}
