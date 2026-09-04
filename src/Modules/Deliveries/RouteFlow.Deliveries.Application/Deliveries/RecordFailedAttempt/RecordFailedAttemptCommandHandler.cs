using FluentValidation;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;

namespace RouteFlow.Deliveries.Application.Deliveries.RecordFailedAttempt;

public sealed class RecordFailedAttemptCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider,
    IValidator<RecordFailedAttemptCommand>? validator = null)
{
    private readonly IValidator<RecordFailedAttemptCommand> _validator = validator ?? new RecordFailedAttemptCommandValidator();

    public async Task HandleAsync(
        RecordFailedAttemptCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        delivery.RecordFailedAttempt(command.Reason, timeProvider.GetUtcNow(), command.Notes);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
