namespace RouteFlow.Deliveries.Domain.ValueObjects;

public sealed record PackageInfo(decimal WeightKg, PackageDimensions Dimensions, string Description);