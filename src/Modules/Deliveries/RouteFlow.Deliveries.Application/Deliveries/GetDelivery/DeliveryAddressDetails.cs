namespace RouteFlow.Deliveries.Application.Deliveries.GetDelivery;

public sealed record DeliveryAddressDetails(
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode);
