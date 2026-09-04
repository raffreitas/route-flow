using FluentValidation;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;

namespace RouteFlow.Deliveries.Application.Deliveries.AuthorizeReturn;

public sealed class AuthorizeReturnCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider,
    IValidator<AuthorizeReturnCommand>? validator = null)
{
    private readonly IValidator<AuthorizeReturnCommand> _validator = validator ?? new AuthorizeReturnCommandValidator();

    public async Task HandleAsync(
        AuthorizeReturnCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        delivery.AuthorizeReturn(command.Reason, timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }
}
