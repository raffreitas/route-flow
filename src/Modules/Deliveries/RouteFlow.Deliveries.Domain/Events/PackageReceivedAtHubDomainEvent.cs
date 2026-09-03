using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain.Events;

public sealed record PackageReceivedAtHubDomainEvent(
    DeliveryId DeliveryId,
    HubId HubId,
    DateTimeOffset OccurredAt) : IDomainEvent;
