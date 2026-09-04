using FluentValidation;

namespace RouteFlow.Fleet.Application.Features.SetDriverAvailability;

public sealed class SetDriverAvailabilityCommandValidator : AbstractValidator<SetDriverAvailabilityCommand>
{
    public SetDriverAvailabilityCommandValidator() => RuleFor(command => command.DriverId.Value).NotEmpty();
}
