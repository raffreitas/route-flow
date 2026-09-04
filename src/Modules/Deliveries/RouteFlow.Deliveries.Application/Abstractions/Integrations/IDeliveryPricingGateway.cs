using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Abstractions.Integrations;

public interface IDeliveryPricingGateway
{
    Task<PriceAdjustmentQuote> QuoteAddressChangeAsync(
        AddressChangePricingRequest request,
        CancellationToken cancellationToken = default);

    Task<PriceAdjustmentQuote> QuoteVehicleRequirementAsync(
        VehicleRequirementPricingRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record AddressChangePricingRequest(
    DeliveryId DeliveryId,
    MerchantId MerchantId,
    PackageInfo Package,
    DeliveryAddress CurrentAddress,
    DeliveryAddress ProposedAddress);

public sealed record VehicleRequirementPricingRequest(
    DeliveryId DeliveryId,
    MerchantId MerchantId,
    PackageInfo Package,
    DeliveryAddress Address,
    VehicleType RequiredVehicleType);

public sealed record PriceAdjustmentQuote(
    Guid QuoteId,
    decimal AdditionalAmount,
    string Currency,
    bool RequiresMerchantApproval);
