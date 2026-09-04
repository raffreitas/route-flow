using FluentValidation;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;

namespace RouteFlow.Deliveries.Application.Deliveries.CancelDelivery;

public sealed class CancelDeliveryCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider,
    IValidator<CancelDeliveryCommand>? validator = null)
{
    private readonly IValidator<CancelDeliveryCommand> _validator = validator ?? new CancelDeliveryCommandValidator();

    public async Task HandleAsync(
        CancelDeliveryCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        delivery.Cancel(command.Reason, timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }
}
