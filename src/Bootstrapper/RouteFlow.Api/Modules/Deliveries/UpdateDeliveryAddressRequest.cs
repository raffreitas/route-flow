using System.ComponentModel.DataAnnotations;
using RouteFlow.Deliveries.Application.Validation;

namespace RouteFlow.Api.Modules.Deliveries;

public sealed record UpdateDeliveryAddressRequest(
    [Required, StringLength(DeliveryValidationLimits.StreetMaxLength)]
    string Street,
    [Required, StringLength(DeliveryValidationLimits.AddressNumberMaxLength)]
    string Number,
    [StringLength(DeliveryValidationLimits.ComplementMaxLength)]
    string? Complement,
    [Required, StringLength(DeliveryValidationLimits.NeighborhoodMaxLength)]
    string Neighborhood,
    [Required, StringLength(DeliveryValidationLimits.CityMaxLength)]
    string City,
    [Required, StringLength(DeliveryValidationLimits.StateLength, MinimumLength = DeliveryValidationLimits.StateLength)]
    string State,
    [Required, StringLength(DeliveryValidationLimits.ZipCodeMaxLength)]
    string ZipCode);
