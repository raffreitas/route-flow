using FluentValidation;
using RouteFlow.Deliveries.Application.Validation;

namespace RouteFlow.Deliveries.Application.Deliveries.UpdateAddressBeforePickup;

public sealed class UpdateAddressBeforePickupCommandValidator : AbstractValidator<UpdateAddressBeforePickupCommand>
{
    public UpdateAddressBeforePickupCommandValidator()
    {
        RuleFor(command => command.DeliveryId.Value).NotEmpty();
        RuleFor(command => command.NewAddress)
            .NotNull()
            .SetValidator(new DeliveryAddressValidator());
    }
}
