namespace RouteFlow.Deliveries.Application.Deliveries.GetDelivery;

public sealed record DeliveryAttemptDetails(
    int AttemptNumber,
    string FailureCategory,
    string FailureDescription,
    DateTimeOffset OccurredAt,
    string? Notes);
