namespace RouteFlow.Deliveries.Application.Deliveries.GetDelivery;

public sealed record PackageDetails(
    decimal WeightKg,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm,
    string Description);
