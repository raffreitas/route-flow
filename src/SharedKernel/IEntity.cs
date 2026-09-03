namespace RouteFlow.SharedKernel;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

public interface IEntity
{
}
