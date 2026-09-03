namespace RouteFlow.Deliveries.Domain.ValueObjects;

public sealed record DeliveryAddress(
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode)
{
    public string Formatted => string.IsNullOrWhiteSpace(Complement)
        ? $"{Street}, {Number} - {Neighborhood}, {City} - {State}, {ZipCode}"
        : $"{Street}, {Number}, {Complement} - {Neighborhood}, {City} - {State}, {ZipCode}";
}