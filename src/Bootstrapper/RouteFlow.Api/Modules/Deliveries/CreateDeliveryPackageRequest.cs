using System.ComponentModel.DataAnnotations;
using RouteFlow.Deliveries.Application.Validation;

namespace RouteFlow.Api.Modules.Deliveries;

public sealed record CreateDeliveryPackageRequest(
    [Range(typeof(decimal), DeliveryValidationLimits.MinimumWeight, DeliveryValidationLimits.MaximumWeight)]
    decimal WeightKg,
    [Range(typeof(decimal), DeliveryValidationLimits.MinimumDimension, DeliveryValidationLimits.MaximumDimension)]
    decimal LengthCm,
    [Range(typeof(decimal), DeliveryValidationLimits.MinimumDimension, DeliveryValidationLimits.MaximumDimension)]
    decimal WidthCm,
    [Range(typeof(decimal), DeliveryValidationLimits.MinimumDimension, DeliveryValidationLimits.MaximumDimension)]
    decimal HeightCm,
    [Required, StringLength(DeliveryValidationLimits.DescriptionMaxLength)]
    string Description);
