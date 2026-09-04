using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.ReportIncompatibleVehicle;

public sealed record ReportIncompatibleVehicleCommand(
    DeliveryId DeliveryId,
    VehicleType RequiredVehicleType,
    string Reason);
