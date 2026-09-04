using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;

namespace RouteFlow.Deliveries.Application.Deliveries.AssignDriver;

public sealed class AssignDriverCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider)
{
    public async Task HandleAsync(AssignDriverCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        var assignedAt = timeProvider.GetUtcNow();
        if (command.VehicleType is { } vehicleType)
        {
            delivery.AssignDriver(command.DriverId, vehicleType, assignedAt);
        }
        else
        {
            delivery.AssignDriver(command.DriverId, assignedAt);
        }

        await repository.SaveChangesAsync(cancellationToken);
    }
}
