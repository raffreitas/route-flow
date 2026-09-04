using System.ComponentModel.DataAnnotations;
using RouteFlow.Deliveries.Domain.Enums;

namespace RouteFlow.Api.Modules.Deliveries;

public sealed record AssignDriverRequest(
    [NotEmptyGuid]
    Guid DriverId,
    [EnumDataType(typeof(VehicleType))]
    string? VehicleType = null);
