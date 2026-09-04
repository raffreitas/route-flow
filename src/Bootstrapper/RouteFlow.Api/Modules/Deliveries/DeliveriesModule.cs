using RouteFlow.Deliveries.Application;
using RouteFlow.Deliveries.Infrastructure;

namespace RouteFlow.Api.Modules.Deliveries;

public static class DeliveriesModule
{
    public static IServiceCollection AddDeliveriesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("deliveries")
            ?? throw new InvalidOperationException("Connection string 'deliveries' is required.");

        services.AddDeliveriesApplication();
        services.AddDeliveriesInfrastructure(connectionString);

        return services;
    }
}
