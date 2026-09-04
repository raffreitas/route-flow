using System.ComponentModel.DataAnnotations;
using RouteFlow.Deliveries.Application.Validation;

namespace RouteFlow.Api.Modules.Deliveries;

public sealed record ReasonRequest(
    [Required, StringLength(DeliveryValidationLimits.ReasonMaxLength)]
    string Reason);
