namespace RouteFlow.Deliveries.Domain.ValueObjects;

public readonly record struct DeliveryId(Guid Value)
{
    public static DeliveryId New() => new(Guid.CreateVersion7());
    public static DeliveryId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

public readonly record struct MerchantId(Guid Value)
{
    public static MerchantId New() => new(Guid.CreateVersion7());
    public static MerchantId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

public readonly record struct DriverId(Guid Value)
{
    public static DriverId New() => new(Guid.CreateVersion7());
    public static DriverId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

public readonly record struct HubId(Guid Value)
{
    public static HubId New() => new(Guid.CreateVersion7());
    public static HubId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}