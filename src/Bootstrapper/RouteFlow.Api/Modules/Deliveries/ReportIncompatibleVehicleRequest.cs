using System.ComponentModel.DataAnnotations;
using RouteFlow.Deliveries.Application.Validation;
using RouteFlow.Deliveries.Domain.Enums;

namespace RouteFlow.Api.Modules.Deliveries;

public sealed record ReportIncompatibleVehicleRequest(
    [Required, EnumDataType(typeof(VehicleType))]
    string RequiredVehicleType,
    [Required, StringLength(DeliveryValidationLimits.ReasonMaxLength)]
    string Reason);
