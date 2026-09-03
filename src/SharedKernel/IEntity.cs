namespace RouteFlow.SharedKernel;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

public interface IEntity
{
}
