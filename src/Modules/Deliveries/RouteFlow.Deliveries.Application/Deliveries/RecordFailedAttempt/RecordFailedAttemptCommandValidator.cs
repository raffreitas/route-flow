using FluentValidation;
using RouteFlow.Deliveries.Application.Validation;

namespace RouteFlow.Deliveries.Application.Deliveries.RecordFailedAttempt;

public sealed class RecordFailedAttemptCommandValidator : AbstractValidator<RecordFailedAttemptCommand>
{
    public RecordFailedAttemptCommandValidator()
    {
        RuleFor(command => command.DeliveryId.Value).NotEmpty();
        RuleFor(command => command.Reason)
            .NotNull()
            .SetValidator(new FailureReasonValidator());
        RuleFor(command => command.Notes)
            .MaximumLength(DeliveryValidationLimits.NotesMaxLength);
    }
}
