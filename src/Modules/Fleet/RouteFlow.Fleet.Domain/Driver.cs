using RouteFlow.Fleet.Domain.Enums;
using RouteFlow.Fleet.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Fleet.Domain;

public sealed class Driver : AggregateRoot<DriverId>
{
    private const int MaxNameLength = 200;

    public string Name { get; private set; }
    public VehicleType VehicleType { get; private set; }
    public bool IsAvailable { get; private set; }
    public DateTimeOffset RegisteredAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private Driver() => Name = null!;

    private Driver(DriverId id, string name, VehicleType vehicleType, DateTimeOffset registeredAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Driver name is required.");
        }

        if (name.Trim().Length > MaxNameLength)
        {
            throw new DomainException($"Driver name cannot exceed {MaxNameLength} characters.");
        }

        if (!Enum.IsDefined(vehicleType))
        {
            throw new DomainException("Driver vehicle type is not supported.");
        }

        Id = id;
        Name = name.Trim();
        VehicleType = vehicleType;
        IsAvailable = false;
        RegisteredAt = registeredAt;
    }

    public static Driver Register(
        DriverId id,
        string name,
        VehicleType vehicleType,
        DateTimeOffset? registeredAt = null)
    {
        if (id.Value == Guid.Empty)
        {
            throw new DomainException("Driver identifier is required.");
        }

        return new Driver(id, name, vehicleType, registeredAt ?? DateTimeOffset.UtcNow);
    }

    public void SetAvailability(bool isAvailable, DateTimeOffset? changedAt = null)
    {
        if (IsAvailable == isAvailable)
        {
            return;
        }

        IsAvailable = isAvailable;
        UpdatedAt = changedAt ?? DateTimeOffset.UtcNow;
    }
}
