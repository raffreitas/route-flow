namespace RouteFlow.Api.Modules.Deliveries;

public sealed record CreateDeliveryAddressRequest(
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode);
