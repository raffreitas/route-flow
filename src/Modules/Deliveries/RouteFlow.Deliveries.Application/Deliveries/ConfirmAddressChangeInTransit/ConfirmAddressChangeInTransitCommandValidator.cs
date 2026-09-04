using FluentValidation;
using RouteFlow.Deliveries.Application.Validation;

namespace RouteFlow.Deliveries.Application.Deliveries.ConfirmAddressChangeInTransit;

public sealed class ConfirmAddressChangeInTransitCommandValidator : AbstractValidator<ConfirmAddressChangeInTransitCommand>
{
    public ConfirmAddressChangeInTransitCommandValidator()
    {
        RuleFor(command => command.DeliveryId.Value).NotEmpty();
        RuleFor(command => command.NewAddress)
            .NotNull()
            .SetValidator(new DeliveryAddressValidator());
    }
}
