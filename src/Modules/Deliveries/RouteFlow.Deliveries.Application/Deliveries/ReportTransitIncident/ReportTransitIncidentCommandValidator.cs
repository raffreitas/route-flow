using FluentValidation;
using RouteFlow.Deliveries.Application.Validation;

namespace RouteFlow.Deliveries.Application.Deliveries.ReportTransitIncident;

public sealed class ReportTransitIncidentCommandValidator : AbstractValidator<ReportTransitIncidentCommand>
{
    public ReportTransitIncidentCommandValidator()
    {
        RuleFor(command => command.DeliveryId.Value).NotEmpty();
        RuleFor(command => command.Reason)
            .NotEmpty()
            .MaximumLength(DeliveryValidationLimits.ReasonMaxLength);
    }
}
