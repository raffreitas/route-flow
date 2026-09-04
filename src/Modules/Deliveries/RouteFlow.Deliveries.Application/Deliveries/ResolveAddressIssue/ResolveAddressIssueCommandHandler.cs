using FluentValidation;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;

namespace RouteFlow.Deliveries.Application.Deliveries.ResolveAddressIssue;

public sealed class ResolveAddressIssueCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider,
    IValidator<ResolveAddressIssueCommand>? validator = null)
{
    private readonly IValidator<ResolveAddressIssueCommand> _validator = validator ?? new ResolveAddressIssueCommandValidator();

    public async Task HandleAsync(
        ResolveAddressIssueCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var delivery = await repository.GetByIdAsync(command.DeliveryId, cancellationToken)
            ?? throw new DeliveryNotFoundException(command.DeliveryId);

        delivery.ResolveAddressIssue(command.CorrectedAddress, timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
    }
}
