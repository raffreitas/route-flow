using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;

namespace RouteFlow.Deliveries.Application.Deliveries.ExpireOperationalIssue;

public sealed class ExpireOperationalIssueCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider)
{
    public async Task HandleAsync(
        ExpireOperationalIssueCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        delivery.ExpireOperationalIssue(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }
}
