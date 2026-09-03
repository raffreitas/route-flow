using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain.Events;

public sealed record DeliveryRequestedDomainEvent(
    DeliveryId DeliveryId,
    MerchantId MerchantId,
    DateTimeOffset OccurredAt) : IDomainEvent;
