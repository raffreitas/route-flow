namespace RouteFlow.Deliveries.Application.Deliveries.RequestDelivery;

public sealed record RequestDeliveryCommand(
    Guid MerchantId,
    DeliveryAddressInput Address,
    PackageInput Package);

public sealed record DeliveryAddressInput(
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode);

public sealed record PackageInput(
    decimal WeightKg,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm,
    string Description);
