namespace RouteFlow.Deliveries.Infrastructure.Persistence.Queries;

internal sealed class DeliveryAttemptQueryRow
{
    public int AttemptNumber { get; set; }
    public string FailureCategory { get; set; } = string.Empty;
    public string FailureDescription { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public string? Notes { get; set; }
}
