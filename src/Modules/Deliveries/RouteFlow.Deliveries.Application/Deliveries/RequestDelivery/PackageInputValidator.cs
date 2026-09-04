using FluentValidation;
using RouteFlow.Deliveries.Application.Validation;

namespace RouteFlow.Deliveries.Application.Deliveries.RequestDelivery;

public sealed class PackageInputValidator : AbstractValidator<PackageInput>
{
    public PackageInputValidator()
    {
        RuleFor(package => package.WeightKg)
            .InclusiveBetween(
                DeliveryValidationLimits.MinimumWeightKg,
                DeliveryValidationLimits.MaximumWeightKg);
        RuleFor(package => package.LengthCm)
            .InclusiveBetween(
                DeliveryValidationLimits.MinimumDimensionCm,
                DeliveryValidationLimits.MaximumDimensionCm);
        RuleFor(package => package.WidthCm)
            .InclusiveBetween(
                DeliveryValidationLimits.MinimumDimensionCm,
                DeliveryValidationLimits.MaximumDimensionCm);
        RuleFor(package => package.HeightCm)
            .InclusiveBetween(
                DeliveryValidationLimits.MinimumDimensionCm,
                DeliveryValidationLimits.MaximumDimensionCm);
        RuleFor(package => package.Description)
            .NotEmpty()
            .MaximumLength(DeliveryValidationLimits.DescriptionMaxLength);
    }
}
