using FluentValidation;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;

namespace RouteFlow.Deliveries.Application.Deliveries.UpdateAddressBeforePickup;

public sealed class UpdateAddressBeforePickupCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider,
    IValidator<UpdateAddressBeforePickupCommand>? validator = null)
{
    private readonly IValidator<UpdateAddressBeforePickupCommand> _validator = validator ?? new UpdateAddressBeforePickupCommandValidator();

    public async Task HandleAsync(
        UpdateAddressBeforePickupCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        delivery.UpdateAddressBeforePickup(command.NewAddress, timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }
}
