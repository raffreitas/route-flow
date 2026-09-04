using FluentValidation;
using RouteFlow.Fleet.Application.Abstractions;
using RouteFlow.Fleet.Application.Exceptions;

namespace RouteFlow.Fleet.Application.Features.SetDriverAvailability;

public sealed class SetDriverAvailabilityCommandHandler(
    IDriverRepository repository,
    TimeProvider timeProvider,
    IValidator<SetDriverAvailabilityCommand>? validator = null)
{
    private readonly IValidator<SetDriverAvailabilityCommand> _validator = validator
        ?? new SetDriverAvailabilityCommandValidator();

    public async Task HandleAsync(SetDriverAvailabilityCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var driver = await repository.GetByIdAsync(command.DriverId, cancellationToken)
            ?? throw new DriverNotFoundException(command.DriverId);

        driver.SetAvailability(command.IsAvailable, timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }
}
