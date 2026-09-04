using System.Text.Json;
using RouteFlow.Deliveries.Contracts.IntegrationEvents;

namespace RouteFlow.Deliveries.Infrastructure.Messaging;

internal static class IntegrationEventSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly IReadOnlyDictionary<string, Type> EventTypes =
        new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            [DeliveryRequestedIntegrationEvent.EventType] = typeof(DeliveryRequestedIntegrationEvent),
            [DeliveryCanceledIntegrationEvent.EventType] = typeof(DeliveryCanceledIntegrationEvent),
            [DeliveryAttemptFailedIntegrationEvent.EventType] = typeof(DeliveryAttemptFailedIntegrationEvent),
            [DeliveryStatusChangedIntegrationEvent.EventType] = typeof(DeliveryStatusChangedIntegrationEvent)
        };

    public static string Serialize(IDeliveriesIntegrationEvent integrationEvent)
    {
        return JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), SerializerOptions);
    }

    public static IIntegrationEventEnvelope DeserializeEnvelope(
        Guid messageId,
        string type,
        DateTimeOffset occurredAt,
        string content)
    {
        if (!EventTypes.TryGetValue(type, out var eventType))
        {
            throw new InvalidOperationException($"Unsupported integration event type '{type}'.");
        }

        var integrationEvent = (IDeliveriesIntegrationEvent)(
            JsonSerializer.Deserialize(content, eventType, SerializerOptions)
            ?? throw new InvalidOperationException($"Integration event '{type}' could not be deserialized."));
        var envelopeType = typeof(IntegrationEventEnvelope<>).MakeGenericType(eventType);

        return (IIntegrationEventEnvelope)Activator.CreateInstance(
            envelopeType,
            messageId,
            type,
            occurredAt,
            integrationEvent)!;
    }
}
