using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain.Events;

public sealed record OperationalIssueExpiredDomainEvent(
    DeliveryId DeliveryId,
    DateTimeOffset OccurredAt) : IDomainEvent;
