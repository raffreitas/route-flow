namespace RouteFlow.Api.Modules.Deliveries;

public sealed record CreateDeliveryPackageRequest(
    decimal WeightKg,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm,
    string Description);
