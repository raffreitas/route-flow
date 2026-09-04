using RouteFlow.Deliveries.Domain.Entities;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.Events;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain;

public sealed class Delivery : AggregateRoot<DeliveryId>
{
    private const int MaxAttemptsForAbsentRecipient = 3;
    private static readonly TimeSpan OperationalIssueResolutionWindow = TimeSpan.FromHours(48);

    private readonly List<DeliveryAttempt> _attempts = [];

    public DeliveryStatus Status { get; private set; }
    public Custody CurrentCustody { get; private set; }
    public MerchantId MerchantId { get; private set; }
    public DriverId? AssignedDriverId { get; private set; }
    public VehicleType? RequiredVehicleType { get; private set; }
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
        AssignDriver(driverId, assignedAt, vehicleType: null);
    }

    public void AssignDriver(DriverId driverId, VehicleType vehicleType, DateTimeOffset? assignedAt = null)
    {
        AssignDriver(driverId, assignedAt, vehicleType);
    }

    private void AssignDriver(DriverId driverId, DateTimeOffset? assignedAt, VehicleType? vehicleType)
    {
        EnsureNotTerminalState();

        if (Status != DeliveryStatus.Requested)
        {
            throw new DomainException(
                $"Cannot assign driver when delivery is in status '{Status}'. Only deliveries in 'Requested' status can be assigned.");
        }

        if (RequiredVehicleType is not null && vehicleType is null)
        {
            throw new DomainException(
                $"Vehicle type '{RequiredVehicleType}' is required to assign a driver to this delivery.");
        }

        if (RequiredVehicleType is not null && vehicleType != RequiredVehicleType)
        {
            throw new DomainException(
                $"Cannot assign vehicle type '{vehicleType}' when delivery requires '{RequiredVehicleType}'.");
        }

        AssignedDriverId = driverId;
        Status = DeliveryStatus.DriverAssigned;
        UpdatedAt = assignedAt ?? DateTimeOffset.UtcNow;

        AddDomainEvent(new DriverAssignedDomainEvent(Id, driverId, UpdatedAt.Value));
    }

    public void StartDispatchToPickup(DateTimeOffset? dispatchedAt = null)
    {
        EnsureNotTerminalState();

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
        EnsureNotTerminalState();

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
        EnsureNotTerminalState();

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

    public void UpdateAddressBeforePickup(DeliveryAddress newAddress, DateTimeOffset? updatedAt = null)
    {
        EnsureNotTerminalState();

        if (CurrentCustody != Custody.Merchant)
        {
            throw new DomainException("Cannot update address as a pre-pickup change after package custody leaves the merchant.");
        }

        UpdateAddress(newAddress, updatedAt ?? DateTimeOffset.UtcNow);
    }

    public void ConfirmAddressChangeInTransit(DeliveryAddress newAddress, DateTimeOffset? confirmedAt = null)
    {
        EnsureNotTerminalState();

        if (Status != DeliveryStatus.InTransit)
        {
            throw new DomainException(
                $"Cannot confirm an in-transit address change when delivery is in status '{Status}'. Must be in 'InTransit' status.");
        }

        UpdateAddress(newAddress, confirmedAt ?? DateTimeOffset.UtcNow);
    }

    public void ReleaseDriverBeforePickup(string reason, DateTimeOffset? releasedAt = null)
    {
        EnsureNotTerminalState();

        if (Status is not (DeliveryStatus.DriverAssigned or DeliveryStatus.DispatchedToPickup))
        {
            throw new DomainException(
                $"Cannot release driver when delivery is in status '{Status}'. Must be in 'DriverAssigned' or 'DispatchedToPickup' status.");
        }

        var driverId = AssignedDriverId
            ?? throw new DomainException("Cannot release driver without an assigned driver.");
        var timestamp = releasedAt ?? DateTimeOffset.UtcNow;

        AssignedDriverId = null;
        Status = DeliveryStatus.Requested;
        UpdatedAt = timestamp;

        AddDomainEvent(new DriverReleasedDomainEvent(Id, driverId, reason, timestamp));
    }

    public void ReportIncompatibleVehicle(
        VehicleType requiredVehicleType,
        string reason,
        DateTimeOffset? reportedAt = null)
    {
        EnsureNotTerminalState();

        if (Status != DeliveryStatus.ArrivedAtPickup)
        {
            throw new DomainException(
                $"Cannot report incompatible vehicle when delivery is in status '{Status}'. Must be in 'ArrivedAtPickup' status.");
        }

        if (!Enum.IsDefined(requiredVehicleType))
        {
            throw new DomainException($"Vehicle type '{requiredVehicleType}' is not supported.");
        }

        var driverId = AssignedDriverId
            ?? throw new DomainException("Cannot report incompatible vehicle without an assigned driver.");
        var timestamp = reportedAt ?? DateTimeOffset.UtcNow;

        AssignedDriverId = null;
        RequiredVehicleType = requiredVehicleType;
        Status = DeliveryStatus.Requested;
        UpdatedAt = timestamp;

        AddDomainEvent(new IncompatibleVehicleReportedDomainEvent(
            Id, driverId, requiredVehicleType, reason, timestamp));
    }

    public void ConfirmDeliveryToRecipient(DateTimeOffset? completedAt = null)
    {
        EnsureNotTerminalState();

        if (Status != DeliveryStatus.InTransit)
        {
            throw new DomainException(
                $"Cannot confirm delivery to recipient when status is '{Status}'. Must be in 'InTransit' status.");
        }

        Status = DeliveryStatus.Completed;
        CurrentCustody = Custody.Recipient;
        UpdatedAt = completedAt ?? DateTimeOffset.UtcNow;

        AddDomainEvent(new DeliveryCompletedDomainEvent(Id, UpdatedAt.Value));
    }

    public void RecordFailedAttempt(FailureReason reason, DateTimeOffset? occurredAt = null, string? notes = null)
    {
        EnsureNotTerminalState();

        if (Status != DeliveryStatus.InTransit)
        {
            throw new DomainException(
                $"Cannot record a failed delivery attempt when status is '{Status}'. Must be in 'InTransit' status.");
        }

        var timestamp = occurredAt ?? DateTimeOffset.UtcNow;
        var attemptNumber = _attempts.Count + 1;
        var attempt = new DeliveryAttempt(attemptNumber, reason, timestamp, notes);
        _attempts.Add(attempt);
        UpdatedAt = timestamp;

        AddDomainEvent(new DeliveryAttemptFailedDomainEvent(Id, attemptNumber, reason, timestamp));

        if (reason.Category == FailureCategory.RecipientRefused)
        {
            Status = DeliveryStatus.InReturn;
            AddDomainEvent(new DeliveryReturnInitiatedDomainEvent(Id, "Recipient refused package", timestamp));
            return;
        }

        if (reason.RequiresOperationalIssueQueue)
        {
            Status = DeliveryStatus.InOperationalIssue;
            AddDomainEvent(new DeliverySentToOperationalIssueDomainEvent(Id, reason, timestamp));
            return;
        }

        if (reason.Category == FailureCategory.RecipientAbsent)
        {
            if (attemptNumber >= MaxAttemptsForAbsentRecipient)
            {
                Status = DeliveryStatus.InReturn;
                AddDomainEvent(new DeliveryReturnInitiatedDomainEvent(
                    Id, $"Maximum delivery attempts reached ({MaxAttemptsForAbsentRecipient})", timestamp));
                return;
            }

            Status = DeliveryStatus.PendingReschedule;
            return;
        }

        Status = DeliveryStatus.PendingReschedule;
    }

    public void ResolveAddressIssue(DeliveryAddress newAddress, DateTimeOffset? resolvedAt = null)
    {
        EnsureNotTerminalState();

        if (Status != DeliveryStatus.InOperationalIssue)
        {
            throw new DomainException(
                $"Cannot resolve address issue when delivery is in status '{Status}'. Must be in 'InOperationalIssue' status.");
        }

        ArgumentNullException.ThrowIfNull(newAddress);
        Status = DeliveryStatus.PendingReschedule;
        var timestamp = resolvedAt ?? DateTimeOffset.UtcNow;
        UpdateAddress(newAddress, timestamp);
    }

    public void AuthorizeReturn(string reason, DateTimeOffset? authorizedAt = null)
    {
        EnsureNotTerminalState();

        if (Status is not (DeliveryStatus.InOperationalIssue or DeliveryStatus.ReceivedAtHub))
        {
            throw new DomainException(
                $"Cannot authorize return when delivery is in status '{Status}'. Must be in 'InOperationalIssue' or 'ReceivedAtHub' status.");
        }

        Status = DeliveryStatus.InReturn;
        var timestamp = authorizedAt ?? DateTimeOffset.UtcNow;
        UpdatedAt = timestamp;

        AddDomainEvent(new DeliveryReturnInitiatedDomainEvent(Id, reason, timestamp));
    }

    public void ExpireOperationalIssue(DateTimeOffset? expiredAt = null)
    {
        EnsureNotTerminalState();

        if (Status != DeliveryStatus.InOperationalIssue)
        {
            throw new DomainException(
                $"Cannot expire operational issue when delivery is in status '{Status}'. Must be in 'InOperationalIssue' status.");
        }

        var timestamp = expiredAt ?? DateTimeOffset.UtcNow;
        var resolutionDeadline = UpdatedAt!.Value.Add(OperationalIssueResolutionWindow);
        if (timestamp < resolutionDeadline)
        {
            throw new DomainException($"Operational issue cannot expire before '{resolutionDeadline:O}'.");
        }

        Status = DeliveryStatus.InReturn;
        UpdatedAt = timestamp;

        AddDomainEvent(new OperationalIssueExpiredDomainEvent(Id, timestamp));
        AddDomainEvent(new DeliveryReturnInitiatedDomainEvent(Id, "Operational issue resolution window expired", timestamp));
    }

    public void DispatchToNewRoute(DriverId? driverId = null, DateTimeOffset? dispatchedAt = null)
    {
        EnsureNotTerminalState();

        if (Status is not (DeliveryStatus.PendingReschedule or DeliveryStatus.ReceivedAtHub))
        {
            throw new DomainException(
                $"Cannot dispatch to new route from status '{Status}'. Must be in 'PendingReschedule' or 'ReceivedAtHub'.");
        }

        if (Status == DeliveryStatus.ReceivedAtHub)
        {
            if (driverId is null)
            {
                throw new DomainException("A driver is required to dispatch a package from the hub.");
            }

            AssignedDriverId = driverId;
            CurrentCustody = Custody.Driver;
        }
        else if (driverId is not null)
        {
            AssignedDriverId = driverId;
        }

        Status = DeliveryStatus.InTransit;
        UpdatedAt = dispatchedAt ?? DateTimeOffset.UtcNow;
    }

    public void ReportTransitIncident(string reason, DateTimeOffset? reportedAt = null)
    {
        EnsureNotTerminalState();

        if (Status != DeliveryStatus.InTransit)
        {
            throw new DomainException(
                $"Cannot report transit incident when delivery is in status '{Status}'. Must be in 'InTransit' status.");
        }

        if (AssignedDriverId is null)
        {
            throw new DomainException("Cannot report transit incident without an assigned driver.");
        }

        Status = DeliveryStatus.HeldDueToIncident;
        var timestamp = reportedAt ?? DateTimeOffset.UtcNow;
        UpdatedAt = timestamp;

        AddDomainEvent(new TransitIncidentReportedDomainEvent(Id, AssignedDriverId.Value, reason, timestamp));
    }

    public void CheckInPackageAtHub(HubId hubId, DateTimeOffset? checkedInAt = null)
    {
        EnsureNotTerminalState();

        if (Status is not (DeliveryStatus.HeldDueToIncident or DeliveryStatus.InTransit
            or DeliveryStatus.PendingReschedule))
        {
            throw new DomainException(
                $"Cannot check in package at hub from status '{Status}'. Must be in 'HeldDueToIncident', 'InTransit' or 'PendingReschedule'.");
        }

        Status = DeliveryStatus.ReceivedAtHub;
        CurrentCustody = Custody.Hub;
        AssignedDriverId = null;
        var timestamp = checkedInAt ?? DateTimeOffset.UtcNow;
        UpdatedAt = timestamp;

        AddDomainEvent(new PackageReceivedAtHubDomainEvent(Id, hubId, timestamp));
    }

    public void Cancel(string reason, DateTimeOffset? canceledAt = null)
    {
        EnsureNotTerminalState();

        var timestamp = canceledAt ?? DateTimeOffset.UtcNow;
        UpdatedAt = timestamp;

        // If package is still in Merchant custody (before physical pickup)
        if (CurrentCustody == Custody.Merchant)
        {
            var statusBeforeCancellation = Status;
            var assignedDriverId = AssignedDriverId;
            AssignedDriverId = null;
            Status = DeliveryStatus.Canceled;

            if (statusBeforeCancellation == DeliveryStatus.ArrivedAtPickup && assignedDriverId is not null)
            {
                AddDomainEvent(new PickupCanceledByMerchantDomainEvent(
                    Id, assignedDriverId.Value, reason, timestamp));
            }
            else
            {
                if (assignedDriverId is not null)
                {
                    AddDomainEvent(new DriverReleasedDomainEvent(
                        Id, assignedDriverId.Value, reason, timestamp));
                }

                AddDomainEvent(new DeliveryCanceledDomainEvent(Id, reason, timestamp));
            }

            return;
        }

        // If package was already collected and is in RouteFlow/Driver/Hub custody, cannot simply cancel!
        Status = DeliveryStatus.InReturn;
        AddDomainEvent(new DeliveryReturnInitiatedDomainEvent(
            Id, $"Canceled while in custody: {reason}", timestamp));
    }

    public void ConfirmReturnToSender(DateTimeOffset? returnedAt = null)
    {
        EnsureNotTerminalState();

        if (Status != DeliveryStatus.InReturn)
        {
            throw new DomainException(
                $"Cannot confirm return to sender when status is '{Status}'. Must be in 'InReturn' status.");
        }

        Status = DeliveryStatus.ReturnedToSender;
        CurrentCustody = Custody.Merchant;
        UpdatedAt = returnedAt ?? DateTimeOffset.UtcNow;

        AddDomainEvent(new DeliveryReturnedToSenderDomainEvent(Id, UpdatedAt.Value));
    }

    private void EnsureNotTerminalState()
    {
        if (Status is DeliveryStatus.Completed or DeliveryStatus.ReturnedToSender or DeliveryStatus.Canceled)
        {
            throw new DomainException(
                $"Cannot modify delivery '{Id}' because it is already in terminal state '{Status}'.");
        }
    }

    private void UpdateAddress(DeliveryAddress newAddress, DateTimeOffset timestamp)
    {
        Address = newAddress ?? throw new ArgumentNullException(nameof(newAddress));
        UpdatedAt = timestamp;
        AddDomainEvent(new DeliveryAddressUpdatedDomainEvent(Id, newAddress, timestamp));
    }
}
