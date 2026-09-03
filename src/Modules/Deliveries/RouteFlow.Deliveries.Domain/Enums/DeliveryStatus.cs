namespace RouteFlow.Deliveries.Domain.Enums;

public enum DeliveryStatus
{
    Requested = 1,
    DriverAssigned = 2,
    DispatchedToPickup = 3,
    ArrivedAtPickup = 4,
    InTransit = 5,
    PendingReschedule = 6,
    InOperationalIssue = 7,
    HeldDueToIncident = 8,
    ReceivedAtHub = 9,
    InReturn = 10,
    Completed = 11,
    ReturnedToSender = 12,
    Canceled = 13
}