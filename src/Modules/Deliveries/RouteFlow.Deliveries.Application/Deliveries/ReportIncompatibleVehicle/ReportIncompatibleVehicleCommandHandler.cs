using FluentValidation;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;

namespace RouteFlow.Deliveries.Application.Deliveries.ReportIncompatibleVehicle;

public sealed class ReportIncompatibleVehicleCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider,
    IValidator<ReportIncompatibleVehicleCommand>? validator = null)
{
    private readonly IValidator<ReportIncompatibleVehicleCommand> _validator = validator ?? new ReportIncompatibleVehicleCommandValidator();

    public async Task HandleAsync(
        ReportIncompatibleVehicleCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        delivery.ReportIncompatibleVehicle(
            command.RequiredVehicleType,
            command.Reason,
            timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }
}
