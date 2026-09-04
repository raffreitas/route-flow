using FluentValidation;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Validation;

public sealed class DeliveryAddressValidator : AbstractValidator<DeliveryAddress>
{
    public DeliveryAddressValidator()
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
