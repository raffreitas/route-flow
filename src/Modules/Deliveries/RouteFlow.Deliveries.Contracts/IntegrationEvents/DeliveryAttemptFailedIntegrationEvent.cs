namespace RouteFlow.Deliveries.Contracts.IntegrationEvents;

public sealed record DeliveryAttemptFailedIntegrationEvent(
    Guid DeliveryId,
    int AttemptNumber,
    string FailureCategory,
    string FailureDescription) : IDeliveriesIntegrationEvent
{
    public const string EventType = "deliveries.delivery-attempt-failed.v1";
}
