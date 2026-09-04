using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Infrastructure.Messaging;
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

        services.AddDbContext<DeliveriesDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "deliveries")));
        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<IDeliveryQueries, DeliveryQueries>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<InProcessIntegrationEventQueue>();
        services.AddScoped<IIntegrationEventPublisher, InProcessIntegrationEventPublisher>();
        services.AddHostedService<InProcessIntegrationEventDispatcher>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}