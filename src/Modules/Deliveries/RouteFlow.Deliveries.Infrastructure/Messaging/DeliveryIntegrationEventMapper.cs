using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RouteFlow.Deliveries.Contracts.IntegrationEvents;
using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.Events;

namespace RouteFlow.Deliveries.Infrastructure.Messaging;

internal static class DeliveryIntegrationEventMapper
{
    public static IEnumerable<(string Type, DateTimeOffset OccurredAt, IDeliveriesIntegrationEvent Event)> Map(
        EntityEntry<Delivery> entry)
    {
        var delivery = entry.Entity;

        foreach (var domainEvent in delivery.DomainEvents)
        {
            (string Type, DateTimeOffset OccurredAt, IDeliveriesIntegrationEvent Event)? mappedEvent = domainEvent switch
            {
                DeliveryRequestedDomainEvent requested =>
                    (DeliveryRequestedIntegrationEvent.EventType, requested.OccurredAt,
                    new DeliveryRequestedIntegrationEvent(
                        requested.DeliveryId.Value,
                        requested.MerchantId.Value)),
                DeliveryCanceledDomainEvent canceled =>
                    (DeliveryCanceledIntegrationEvent.EventType, canceled.OccurredAt,
                    new DeliveryCanceledIntegrationEvent(
                        canceled.DeliveryId.Value,
                        canceled.Reason)),
                DeliveryAttemptFailedDomainEvent failed =>
                    (DeliveryAttemptFailedIntegrationEvent.EventType, failed.OccurredAt,
                    new DeliveryAttemptFailedIntegrationEvent(
                        failed.DeliveryId.Value,
                        failed.AttemptNumber,
                        failed.Reason.Category.ToString(),
                        failed.Reason.Description)),
                _ => null
            };

            if (mappedEvent is { } value)
            {
                yield return value;
            }
        }

        if (entry.State == EntityState.Added || entry.Property(value => value.Status).IsModified)
        {
            var previousStatus = entry.State == EntityState.Added
                ? null
                : entry.Property(value => value.Status).OriginalValue.ToString();
            var occurredAt = delivery.UpdatedAt ?? delivery.CreatedAt;

            yield return (
                DeliveryStatusChangedIntegrationEvent.EventType,
                occurredAt,
                new DeliveryStatusChangedIntegrationEvent(
                    delivery.Id.Value,
                    previousStatus,
                    delivery.Status.ToString(),
                    delivery.CurrentCustody.ToString(),
                    delivery.AssignedDriverId?.Value));
        }
    }
}
