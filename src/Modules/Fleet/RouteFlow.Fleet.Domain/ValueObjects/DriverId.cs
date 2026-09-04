namespace RouteFlow.Fleet.Domain.ValueObjects;

public readonly record struct DriverId(Guid Value)
{
    public static DriverId New() => new(Guid.CreateVersion7());
    public static DriverId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
