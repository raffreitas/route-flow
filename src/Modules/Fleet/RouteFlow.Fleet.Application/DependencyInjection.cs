using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RouteFlow.Fleet.Application.Features.GetDriverAvailability;
using RouteFlow.Fleet.Application.Features.RegisterDriver;
using RouteFlow.Fleet.Application.Features.SetDriverAvailability;
using RouteFlow.Fleet.Contracts.DriverAvailability;

namespace RouteFlow.Fleet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFleetApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IValidator<RegisterDriverCommand>, RegisterDriverCommandValidator>();
        services.AddScoped<IValidator<SetDriverAvailabilityCommand>, SetDriverAvailabilityCommandValidator>();
        services.AddScoped<RegisterDriverCommandHandler>();
        services.AddScoped<SetDriverAvailabilityCommandHandler>();
        services.AddScoped<GetDriverAvailabilityQueryHandler>();
        services.AddScoped<IDriverAvailabilityReader>(provider =>
            provider.GetRequiredService<GetDriverAvailabilityQueryHandler>());
        return services;
    }
}
