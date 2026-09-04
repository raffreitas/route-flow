using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.ReportTransitIncident;

public sealed record ReportTransitIncidentCommand(DeliveryId DeliveryId, string Reason);
