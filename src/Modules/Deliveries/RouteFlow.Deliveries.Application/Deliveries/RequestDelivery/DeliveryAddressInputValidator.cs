using FluentValidation;
using RouteFlow.Deliveries.Application.Validation;

namespace RouteFlow.Deliveries.Application.Deliveries.RequestDelivery;

public sealed class DeliveryAddressInputValidator : AbstractValidator<DeliveryAddressInput>
{
    public DeliveryAddressInputValidator()
    {
        RuleFor(address => address.Street)
            .NotEmpty()
            .MaximumLength(DeliveryValidationLimits.StreetMaxLength);
        RuleFor(address => address.Number)
            .NotEmpty()
            .MaximumLength(DeliveryValidationLimits.AddressNumberMaxLength);
        RuleFor(address => address.Complement)
            .MaximumLength(DeliveryValidationLimits.ComplementMaxLength);
        RuleFor(address => address.Neighborhood)
            .NotEmpty()
            .MaximumLength(DeliveryValidationLimits.NeighborhoodMaxLength);
        RuleFor(address => address.City)
            .NotEmpty()
            .MaximumLength(DeliveryValidationLimits.CityMaxLength);
        RuleFor(address => address.State)
            .NotEmpty()
            .Length(DeliveryValidationLimits.StateLength);
        RuleFor(address => address.ZipCode)
            .NotEmpty()
            .MaximumLength(DeliveryValidationLimits.ZipCodeMaxLength);
    }
}
