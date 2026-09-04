using FluentValidation;
using RouteFlow.Deliveries.Application.Validation;

namespace RouteFlow.Deliveries.Application.Deliveries.ReportIncompatibleVehicle;

public sealed class ReportIncompatibleVehicleCommandValidator : AbstractValidator<ReportIncompatibleVehicleCommand>
{
    public ReportIncompatibleVehicleCommandValidator()
    {
        RuleFor(command => command.DeliveryId.Value).NotEmpty();
        RuleFor(command => command.RequiredVehicleType).IsInEnum();
        RuleFor(command => command.Reason)
            .NotEmpty()
            .MaximumLength(DeliveryValidationLimits.ReasonMaxLength);
    }
}
