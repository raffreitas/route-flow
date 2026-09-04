namespace RouteFlow.Api.Modules.Deliveries;

public sealed record CreateDeliveryRequest(
    Guid MerchantId,
    CreateDeliveryAddressRequest Address,
    CreateDeliveryPackageRequest Package);
