namespace RouteFlow.Deliveries.Application.Deliveries.GetDelivery;

public sealed record DeliveryDetails(
    Guid Id,
    Guid MerchantId,
    string Status,
    string Custody,
    Guid? AssignedDriverId,
    string? RequiredVehicleType,
    DeliveryAddressDetails Address,
    PackageDetails Package,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    uint Version,
    IReadOnlyCollection<DeliveryAttemptDetails> Attempts);
