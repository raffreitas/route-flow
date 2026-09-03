using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;

namespace RouteFlow.Deliveries.Application.Deliveries.ConfirmPickup;

public sealed class ConfirmPickupCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider)
{
    public async Task HandleAsync(ConfirmPickupCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        delivery.ConfirmPickup(command.DriverId, timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }
}
