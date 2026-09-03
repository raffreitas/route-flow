using RouteFlow.Deliveries.Domain.Entities;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.Events;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain;

public sealed class Delivery : AggregateRoot<DeliveryId>
{
    private readonly List<DeliveryAttempt> _attempts = [];

    public DeliveryStatus Status { get; private set; }
    public Custody CurrentCustody { get; private set; }
    public MerchantId MerchantId { get; private set; }
    public DriverId? AssignedDriverId { get; private set; }
    public DeliveryAddress Address { get; private set; }
    public PackageInfo Package { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<DeliveryAttempt> Attempts => _attempts.AsReadOnly();

    // Required by ORMs like EF Core
    private Delivery()
    {
        Address = null!;
        Package = null!;
    }

    private Delivery(
        DeliveryId id,
        MerchantId merchantId,
        DeliveryAddress address,
        PackageInfo package,
        DateTimeOffset createdAt)
    {
        Id = id;
        MerchantId = merchantId;
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Package = package ?? throw new ArgumentNullException(nameof(package));
        Status = DeliveryStatus.Requested;
        CurrentCustody = Custody.Merchant;
        CreatedAt = createdAt;

        AddDomainEvent(new DeliveryRequestedDomainEvent(Id, MerchantId, CreatedAt));
    }

    public static Delivery Request(
        DeliveryId id,
        MerchantId merchantId,
        DeliveryAddress address,
        PackageInfo package,
        DateTimeOffset? createdAt = null)
    {
        return new Delivery(id, merchantId, address, package, createdAt ?? DateTimeOffset.UtcNow);
    }

    public void AssignDriver(DriverId driverId, DateTimeOffset? assignedAt = null)
    {
        if (Status != DeliveryStatus.Requested)
        {
            throw new DomainException(
                $"Cannot assign driver when delivery is in status '{Status}'. Only deliveries in 'Requested' status can be assigned.");
        }

        AssignedDriverId = driverId;
        Status = DeliveryStatus.DriverAssigned;
        UpdatedAt = assignedAt ?? DateTimeOffset.UtcNow;

        AddDomainEvent(new DriverAssignedDomainEvent(Id, driverId, UpdatedAt.Value));
    }

    public void StartDispatchToPickup(DateTimeOffset? dispatchedAt = null)
    {
        if (Status != DeliveryStatus.DriverAssigned)
        {
            throw new DomainException(
                $"Cannot dispatch to pickup when delivery is in status '{Status}'. Must be in 'DriverAssigned' status.");
        }

        Status = DeliveryStatus.DispatchedToPickup;
        UpdatedAt = dispatchedAt ?? DateTimeOffset.UtcNow;
    }

    public void ConfirmArrivalAtPickup(DateTimeOffset? arrivedAt = null)
    {
        if (Status != DeliveryStatus.DispatchedToPickup)
        {
            throw new DomainException(
                $"Cannot confirm arrival when delivery is in status '{Status}'. Must be in 'DispatchedToPickup' status.");
        }

        Status = DeliveryStatus.ArrivedAtPickup;
        UpdatedAt = arrivedAt ?? DateTimeOffset.UtcNow;
    }

    public void ConfirmPickup(DriverId driverId, DateTimeOffset? pickedUpAt = null)
    {
        if (Status != DeliveryStatus.ArrivedAtPickup)
        {
            throw new DomainException(
                $"Cannot confirm pickup when delivery is in status '{Status}'. Must be in 'ArrivedAtPickup' status.");
        }

        if (AssignedDriverId != driverId)
        {
            throw new DomainException(
                $"Driver '{driverId}' is not assigned to this delivery. Only assigned driver '{AssignedDriverId}' can pick up the package.");
        }

        Status = DeliveryStatus.InTransit;
        CurrentCustody = Custody.Driver;
        UpdatedAt = pickedUpAt ?? DateTimeOffset.UtcNow;

        AddDomainEvent(new PackagePickedUpDomainEvent(Id, driverId, UpdatedAt.Value));
    }
}