using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;

namespace RouteFlow.Deliveries.Application.Deliveries.CheckInPackageAtHub;

public sealed class CheckInPackageAtHubCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider)
{
    public async Task HandleAsync(
        CheckInPackageAtHubCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        delivery.CheckInPackageAtHub(command.HubId, timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }
}
