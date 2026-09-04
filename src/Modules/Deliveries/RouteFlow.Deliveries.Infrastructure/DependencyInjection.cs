using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Abstractions.Integrations;
using RouteFlow.Deliveries.Infrastructure.Messaging;
using RouteFlow.Deliveries.Infrastructure.Integrations;
using RouteFlow.Deliveries.Infrastructure.Persistence;
using RouteFlow.Deliveries.Infrastructure.Persistence.Queries;

namespace RouteFlow.Deliveries.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDeliveriesInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.ConfigureTracing(options =>
        {
            options.ConfigureCommandFilter(commandFilter =>
                !commandFilter.CommandText.Contains("-- outbox-poll", StringComparison.Ordinal));
        });
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<DeliveriesDbContext>(options =>
            options.UseNpgsql(
                dataSource,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "deliveries");
                    npgsqlOptions.EnableRetryOnFailure();
                }));
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<IDeliveryQueries, DeliveryQueries>();
        services.AddScoped<IDriverAvailabilityGateway, FleetDriverAvailabilityGateway>();
        services.AddMetrics();
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<OutboxMetrics>();
        services.AddSingleton<InProcessIntegrationEventQueue>();
        services.AddScoped<IIntegrationEventPublisher, InProcessIntegrationEventPublisher>();
        services.AddHostedService<InProcessIntegrationEventDispatcher>();
        services.AddHostedService<OutboxProcessor>();
        services.AddHealthChecks()
            .AddCheck<DeliveriesOutboxHealthCheck>(
                "deliveries-outbox",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready"]);

        return services;
    }
}
