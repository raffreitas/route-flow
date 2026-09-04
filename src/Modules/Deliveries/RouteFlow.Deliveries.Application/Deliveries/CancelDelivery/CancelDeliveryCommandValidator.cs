using FluentValidation;
using RouteFlow.Deliveries.Application.Validation;

namespace RouteFlow.Deliveries.Application.Deliveries.CancelDelivery;

public sealed class CancelDeliveryCommandValidator : AbstractValidator<CancelDeliveryCommand>
{
    public CancelDeliveryCommandValidator()
    {
        RuleFor(command => command.DeliveryId.Value).NotEmpty();
        RuleFor(command => command.Reason)
            .NotEmpty()
            .MaximumLength(DeliveryValidationLimits.ReasonMaxLength);
    }
}
