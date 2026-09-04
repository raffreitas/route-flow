using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Infrastructure.Persistence;

namespace RouteFlow.Deliveries.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDeliveriesInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<DeliveriesDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "deliveries")));
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();

        return services;
    }
}
