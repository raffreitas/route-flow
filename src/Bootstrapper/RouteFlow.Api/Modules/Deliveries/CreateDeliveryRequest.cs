using System.ComponentModel.DataAnnotations;

namespace RouteFlow.Api.Modules.Deliveries;

public sealed record CreateDeliveryRequest(
    [Required, NotEmptyGuid] Guid MerchantId,
    [Required] CreateDeliveryAddressRequest Address,
    [Required] CreateDeliveryPackageRequest Package);