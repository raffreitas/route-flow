using FluentValidation;
using RouteFlow.Deliveries.Application.Validation;

namespace RouteFlow.Deliveries.Application.Deliveries.AuthorizeReturn;

public sealed class AuthorizeReturnCommandValidator : AbstractValidator<AuthorizeReturnCommand>
{
    public AuthorizeReturnCommandValidator()
    {
        RuleFor(command => command.DeliveryId.Value).NotEmpty();
        RuleFor(command => command.Reason)
            .NotEmpty()
            .MaximumLength(DeliveryValidationLimits.ReasonMaxLength);
    }
}
