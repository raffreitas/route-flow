using FluentValidation;
using RouteFlow.Deliveries.Application.Validation;

namespace RouteFlow.Deliveries.Application.Deliveries.ReleaseDriverBeforePickup;

public sealed class ReleaseDriverBeforePickupCommandValidator : AbstractValidator<ReleaseDriverBeforePickupCommand>
{
    public ReleaseDriverBeforePickupCommandValidator()
    {
        RuleFor(command => command.DeliveryId.Value).NotEmpty();
        RuleFor(command => command.Reason)
            .NotEmpty()
            .MaximumLength(DeliveryValidationLimits.ReasonMaxLength);
    }
}
