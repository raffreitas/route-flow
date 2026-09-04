using FluentValidation;
using RouteFlow.Fleet.Application.Abstractions;
using RouteFlow.Fleet.Domain;
using RouteFlow.Fleet.Domain.Enums;
using RouteFlow.Fleet.Domain.ValueObjects;

namespace RouteFlow.Fleet.Application.Features.RegisterDriver;

public sealed class RegisterDriverCommandHandler(
    IDriverRepository repository,
    TimeProvider timeProvider,
    IValidator<RegisterDriverCommand>? validator = null)
{
    private readonly IValidator<RegisterDriverCommand> _validator = validator ?? new RegisterDriverCommandValidator();

    public async Task<DriverId> HandleAsync(RegisterDriverCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var driver = Driver.Register(
            DriverId.New(),
            command.Name,
            Enum.Parse<VehicleType>(command.VehicleType, ignoreCase: true),
            timeProvider.GetUtcNow());

        await repository.AddAsync(driver, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return driver.Id;
    }
}
