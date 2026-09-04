using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;

namespace RouteFlow.Deliveries.Application.Deliveries.ConfirmDeliveryToRecipient;

public sealed class ConfirmDeliveryToRecipientCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider)
{
    public async Task HandleAsync(
        ConfirmDeliveryToRecipientCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        delivery.ConfirmDeliveryToRecipient(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }
}
