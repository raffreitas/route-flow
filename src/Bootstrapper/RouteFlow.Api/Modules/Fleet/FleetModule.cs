using RouteFlow.Fleet.Application;
using RouteFlow.Fleet.Infrastructure;

namespace RouteFlow.Api.Modules.Fleet;

public static class FleetModule
{
    public static IServiceCollection AddFleetModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("fleet")
            ?? throw new InvalidOperationException("Connection string 'fleet' is required.");

        services.AddFleetApplication();
        services.AddFleetInfrastructure(connectionString);
        return services;
    }
}
