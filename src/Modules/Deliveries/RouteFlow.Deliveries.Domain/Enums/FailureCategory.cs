namespace RouteFlow.Deliveries.Domain.Enums;

public enum FailureCategory
{
    RecipientAbsent = 1,
    AddressNotFound = 2,
    RecipientRefused = 3,
    VehicleBreakdown = 4,
    AccessRestricted = 5
}