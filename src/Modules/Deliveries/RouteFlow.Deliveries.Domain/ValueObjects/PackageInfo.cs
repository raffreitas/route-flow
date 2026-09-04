namespace RouteFlow.Deliveries.Domain.ValueObjects;

public sealed record PackageInfo(decimal WeightKg, PackageDimensions Dimensions, string Description)
{
    // Required by ORMs like EF Core
    private PackageInfo() : this(default, null!, null!)
    {
    }
}
