using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;

namespace RouteFlow.Deliveries.Application.Deliveries.ConfirmArrivalAtPickup;

public sealed class ConfirmArrivalAtPickupCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider)
{
    public async Task HandleAsync(ConfirmArrivalAtPickupCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        delivery.ConfirmArrivalAtPickup(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }
}
