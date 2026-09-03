using RouteFlow.Deliveries.Domain.Entities;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.Events;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.SharedKernel;

namespace RouteFlow.Deliveries.Domain;

public sealed class Delivery : AggregateRoot<DeliveryId>
{
    private const int MaxAttemptsForAbsentRecipient = 3;

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
        EnsureNotTerminalState();

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

    public void DispatchToNewRoute(DateTimeOffset? dispatchedAt = null)
    {
        EnsureNotTerminalState();

        if (Status is not (DeliveryStatus.PendingReschedule or DeliveryStatus.InOperationalIssue or DeliveryStatus.ReceivedAtHub))
        {
            throw new DomainException(
                $"Cannot dispatch to new route from status '{Status}'. Must be in 'PendingReschedule', 'InOperationalIssue' or 'ReceivedAtHub'.");
        }

        Status = DeliveryStatus.InTransit;
        UpdatedAt = dispatchedAt ?? DateTimeOffset.UtcNow;
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
}