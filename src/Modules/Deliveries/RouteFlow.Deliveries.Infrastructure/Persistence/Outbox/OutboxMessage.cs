namespace RouteFlow.Deliveries.Infrastructure.Persistence.Outbox;

internal sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(
        Guid id,
        Guid aggregateId,
        DateTimeOffset occurredAt,
        string type,
        string content)
    {
        Id = id;
        AggregateId = aggregateId;
        OccurredAt = occurredAt;
        Type = type;
        Content = content;
    }

    public Guid Id { get; private set; }
    public Guid AggregateId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? Error { get; private set; }

    public static OutboxMessage Create(
        Guid aggregateId,
        DateTimeOffset occurredAt,
        string type,
        string content)
    {
        return new OutboxMessage(Guid.CreateVersion7(), aggregateId, occurredAt, type, content);
    }
}
