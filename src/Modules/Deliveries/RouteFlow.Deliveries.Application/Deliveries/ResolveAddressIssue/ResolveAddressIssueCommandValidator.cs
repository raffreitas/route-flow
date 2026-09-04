using FluentValidation;
using RouteFlow.Deliveries.Application.Validation;

namespace RouteFlow.Deliveries.Application.Deliveries.ResolveAddressIssue;

public sealed class ResolveAddressIssueCommandValidator : AbstractValidator<ResolveAddressIssueCommand>
{
    public ResolveAddressIssueCommandValidator()
    {
        RuleFor(command => command.DeliveryId.Value).NotEmpty();
        RuleFor(command => command.CorrectedAddress)
            .NotNull()
            .SetValidator(new DeliveryAddressValidator());
    }
}
