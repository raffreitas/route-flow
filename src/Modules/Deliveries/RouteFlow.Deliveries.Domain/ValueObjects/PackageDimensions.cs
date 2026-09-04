namespace RouteFlow.Deliveries.Domain.ValueObjects;

public sealed record PackageDimensions(decimal LengthCm, decimal WidthCm, decimal HeightCm)
{
    // Required by ORMs like EF Core
    private PackageDimensions() : this(default, default, default)
    {
    }

    public decimal VolumeM3 => (LengthCm * WidthCm * HeightCm) / 1_000_000m;
}
