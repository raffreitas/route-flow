using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RouteFlow.Fleet.Application.Abstractions;
using RouteFlow.Fleet.Infrastructure.Persistence;

namespace RouteFlow.Fleet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFleetInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        services.AddDbContext<FleetDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "fleet");
                    npgsqlOptions.EnableRetryOnFailure();
                }));
        services.AddScoped<IDriverRepository, DriverRepository>();
        return services;
    }
}
