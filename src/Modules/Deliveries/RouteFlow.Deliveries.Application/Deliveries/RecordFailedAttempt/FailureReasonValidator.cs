using FluentValidation;
using RouteFlow.Deliveries.Application.Validation;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.RecordFailedAttempt;

public sealed class FailureReasonValidator : AbstractValidator<FailureReason>
{
    public FailureReasonValidator()
    {
        RuleFor(reason => reason.Category).IsInEnum();
        RuleFor(reason => reason.Description)
            .NotEmpty()
            .MaximumLength(DeliveryValidationLimits.DescriptionMaxLength);
    }
}
