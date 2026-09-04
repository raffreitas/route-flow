using FluentValidation;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;

namespace RouteFlow.Deliveries.Application.Deliveries.ConfirmAddressChangeInTransit;

public sealed class ConfirmAddressChangeInTransitCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider,
    IValidator<ConfirmAddressChangeInTransitCommand>? validator = null)
{
    private readonly IValidator<ConfirmAddressChangeInTransitCommand> _validator = validator ?? new ConfirmAddressChangeInTransitCommandValidator();

    public async Task HandleAsync(
        ConfirmAddressChangeInTransitCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        delivery.ConfirmAddressChangeInTransit(command.NewAddress, timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }
}
