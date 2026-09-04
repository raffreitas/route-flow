using FluentValidation;
using RouteFlow.Fleet.Domain.Enums;

namespace RouteFlow.Fleet.Application.Features.RegisterDriver;

public sealed class RegisterDriverCommandValidator : AbstractValidator<RegisterDriverCommand>
{
    public RegisterDriverCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.VehicleType)
            .NotEmpty()
            .Must(value => Enum.GetNames<VehicleType>().Contains(value, StringComparer.OrdinalIgnoreCase))
            .WithMessage("The vehicle type is not supported.");
    }
}
