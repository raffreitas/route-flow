using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.ExpireOperationalIssue;

public sealed record ExpireOperationalIssueCommand(DeliveryId DeliveryId);
