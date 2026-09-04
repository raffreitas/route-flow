using FluentValidation;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;

namespace RouteFlow.Deliveries.Application.Deliveries.ReleaseDriverBeforePickup;

public sealed class ReleaseDriverBeforePickupCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider,
    IValidator<ReleaseDriverBeforePickupCommand>? validator = null)
{
    private readonly IValidator<ReleaseDriverBeforePickupCommand> _validator = validator ?? new ReleaseDriverBeforePickupCommandValidator();

    public async Task HandleAsync(
        ReleaseDriverBeforePickupCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        delivery.ReleaseDriverBeforePickup(command.Reason, timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }
}
