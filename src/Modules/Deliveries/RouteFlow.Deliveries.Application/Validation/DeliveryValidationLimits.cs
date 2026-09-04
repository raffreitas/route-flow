namespace RouteFlow.Deliveries.Application.Validation;

public static class DeliveryValidationLimits
{
    public const int StreetMaxLength = 200;
    public const int AddressNumberMaxLength = 30;
    public const int ComplementMaxLength = 100;
    public const int NeighborhoodMaxLength = 100;
    public const int CityMaxLength = 100;
    public const int StateLength = 2;
    public const int ZipCodeMaxLength = 16;
    public const int DescriptionMaxLength = 500;
    public const int ReasonMaxLength = 500;
    public const int NotesMaxLength = 1000;
    public const decimal MinimumWeightKg = 0.001m;
    public const decimal MaximumWeightKg = 9999999.999m;
    public const decimal MinimumDimensionCm = 0.01m;
    public const decimal MaximumDimensionCm = 99999999.99m;
    public const string MinimumWeight = "0.001";
    public const string MaximumWeight = "9999999.999";
    public const string MinimumDimension = "0.01";
    public const string MaximumDimension = "99999999.99";
}
