using System.ComponentModel.DataAnnotations;
using RouteFlow.Deliveries.Application.Validation;
using RouteFlow.Deliveries.Domain.Enums;

namespace RouteFlow.Api.Modules.Deliveries;

public sealed record ReportFailedAttemptRequest(
    [Required, EnumDataType(typeof(FailureCategory))]
    string Category,
    [Required, StringLength(DeliveryValidationLimits.DescriptionMaxLength)]
    string Description,
    [StringLength(DeliveryValidationLimits.NotesMaxLength)]
    string? Notes = null);
