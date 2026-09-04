namespace RouteFlow.Deliveries.Infrastructure.Persistence.Queries;

internal sealed class DeliveryQueryRow
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Custody { get; set; } = string.Empty;
    public Guid? AssignedDriverId { get; set; }
    public string? RequiredVehicleType { get; set; }
    public string AddressStreet { get; set; } = string.Empty;
    public string AddressNumber { get; set; } = string.Empty;
    public string? AddressComplement { get; set; }
    public string AddressNeighborhood { get; set; } = string.Empty;
    public string AddressCity { get; set; } = string.Empty;
    public string AddressState { get; set; } = string.Empty;
    public string AddressZipCode { get; set; } = string.Empty;
    public decimal PackageWeightKg { get; set; }
    public decimal PackageLengthCm { get; set; }
    public decimal PackageWidthCm { get; set; }
    public decimal PackageHeightCm { get; set; }
    public string PackageDescription { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public long Version { get; set; }
}
