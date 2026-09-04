using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.ResolveAddressIssue;

public sealed record ResolveAddressIssueCommand(
    DeliveryId DeliveryId,
    DeliveryAddress CorrectedAddress);
