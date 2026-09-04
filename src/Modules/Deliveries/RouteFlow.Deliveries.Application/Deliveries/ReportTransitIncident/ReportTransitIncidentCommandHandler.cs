using FluentValidation;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;

namespace RouteFlow.Deliveries.Application.Deliveries.ReportTransitIncident;

public sealed class ReportTransitIncidentCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider,
    IValidator<ReportTransitIncidentCommand>? validator = null)
{
    private readonly IValidator<ReportTransitIncidentCommand> _validator = validator ?? new ReportTransitIncidentCommandValidator();

    public async Task HandleAsync(
        ReportTransitIncidentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        delivery.ReportTransitIncident(command.Reason, timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }
}
