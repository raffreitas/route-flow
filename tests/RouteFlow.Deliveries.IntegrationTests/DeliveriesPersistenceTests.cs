using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;
using RouteFlow.Deliveries.Contracts.IntegrationEvents;
using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;
using RouteFlow.Deliveries.Infrastructure;

namespace RouteFlow.Deliveries.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class DeliveriesPersistenceTests(PostgreSqlFixture fixture)
{
    private static readonly DateTimeOffset Now = new(2026, 9, 4, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Migrations_WhenDatabaseStartsEmpty_ShouldApplyAllDeliveriesMigrations()
    {
        // Arrange
        await using var dbContext = fixture.CreateDbContext();

        // Act
        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();

        // Assert
        Assert.Contains(appliedMigrations, migration => migration.EndsWith("_InitialDeliveries"));
        Assert.Contains(appliedMigrations, migration => migration.EndsWith("_AddDeliveriesOutbox"));
    }

    [Fact]
    public async Task Repository_WhenAggregateHasDomainEvents_ShouldPersistOutboxMessage()
    {
        // Arrange
        var delivery = CreateRequestedDelivery();
        await using (var serviceProvider = CreateServiceProvider())
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
            await repository.AddAsync(delivery);

            // Act
            await repository.SaveChangesAsync();
        }

        // Assert
        Assert.Empty(delivery.DomainEvents);

        await using var dbContext = fixture.CreateDbContext();
        var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT type, content::text, occurred_at
            FROM deliveries.outbox_messages
            WHERE aggregate_id = @aggregateId AND type = @eventType
            """;
        var aggregateId = command.CreateParameter();
        aggregateId.ParameterName = "aggregateId";
        aggregateId.Value = delivery.Id.Value;
        command.Parameters.Add(aggregateId);
        var eventType = command.CreateParameter();
        eventType.ParameterName = "eventType";
        eventType.Value = DeliveryRequestedIntegrationEvent.EventType;
        command.Parameters.Add(eventType);

        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal(
            DeliveryRequestedIntegrationEvent.EventType,
            reader.GetString(0));
        Assert.Equal(Now, reader.GetFieldValue<DateTimeOffset>(2));

        using var content = JsonDocument.Parse(reader.GetString(1));
        Assert.Equal(
            delivery.Id.Value,
            content.RootElement.GetProperty("deliveryId").GetGuid());
        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task OutboxProcessor_WhenHandlerFailsOnce_ShouldRetryEnvelopeAndMarkMessageProcessed()
    {
        // Arrange
        var delivery = CreateRequestedDelivery();
        var handler = new RetryOnceDeliveryRequestedHandler(delivery.Id.Value);
        await using var serviceProvider = CreateServiceProvider(services =>
            services.AddSingleton<IDeliveriesIntegrationEventHandler<DeliveryRequestedIntegrationEvent>>(handler));
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
            await repository.AddAsync(delivery);
            await repository.SaveChangesAsync();
        }

        var hostedServices = serviceProvider.GetServices<IHostedService>().ToArray();

        // Act
        foreach (var hostedService in hostedServices)
        {
            await hostedService.StartAsync(CancellationToken.None);
        }

        try
        {
            var envelope = await handler.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            var outboxState = await WaitForProcessedOutboxMessageAsync(
                delivery.Id,
                DeliveryRequestedIntegrationEvent.EventType);

            // Assert
            Assert.Equal(outboxState.MessageId, envelope.MessageId);
            Assert.Equal(DeliveryRequestedIntegrationEvent.EventType, envelope.EventType);
            Assert.Equal(Now, envelope.OccurredAt);
            Assert.Equal(delivery.Id.Value, envelope.Event.DeliveryId);
            Assert.Equal(delivery.MerchantId.Value, envelope.Event.MerchantId);
            Assert.Equal(2, outboxState.AttemptCount);
            Assert.NotNull(outboxState.ProcessedAt);
            Assert.Null(outboxState.Error);
        }
        finally
        {
            foreach (var hostedService in hostedServices.Reverse())
            {
                await hostedService.StopAsync(CancellationToken.None);
            }
        }
    }

    [Fact]
    public async Task Repository_WhenDeliveryHasAttempts_ShouldRoundTripCompleteAggregate()
    {
        // Arrange
        var delivery = CreateDeliveryWithFailedAttempt();
        await using (var serviceProvider = CreateServiceProvider())
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
            await repository.AddAsync(delivery);
            await repository.SaveChangesAsync();
        }

        // Act
        Delivery? reloaded;
        await using (var serviceProvider = CreateServiceProvider())
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
            reloaded = await repository.GetByIdAsync(delivery.Id);
        }

        // Assert
        Assert.NotNull(reloaded);
        Assert.Equal(delivery.Id, reloaded.Id);
        Assert.Equal(delivery.MerchantId, reloaded.MerchantId);
        Assert.Equal(delivery.Address, reloaded.Address);
        Assert.Equal(delivery.Package, reloaded.Package);
        Assert.Equal(DeliveryStatus.PendingReschedule, reloaded.Status);
        Assert.Equal(Custody.Driver, reloaded.CurrentCustody);
        Assert.Equal(delivery.AssignedDriverId, reloaded.AssignedDriverId);
        Assert.Equal(1u, reloaded.Version);
        Assert.Empty(reloaded.DomainEvents);

        var attempt = Assert.Single(reloaded.Attempts);
        Assert.Equal(1, attempt.AttemptNumber);
        Assert.Equal(FailureCategory.RecipientAbsent, attempt.Reason.Category);
        Assert.Equal("No answer at the door", attempt.Notes);
    }

    [Fact]
    public async Task Queries_WhenDeliveryExists_ShouldProjectCompleteDetailsWithDapper()
    {
        // Arrange
        var delivery = CreateDeliveryWithFailedAttempt();
        await using var serviceProvider = CreateServiceProvider();
        await using (var writeScope = serviceProvider.CreateAsyncScope())
        {
            var repository = writeScope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
            await repository.AddAsync(delivery);
            await repository.SaveChangesAsync();
        }

        // Act
        await using var readScope = serviceProvider.CreateAsyncScope();
        var queries = readScope.ServiceProvider.GetRequiredService<IDeliveryQueries>();
        var details = await queries.GetByIdAsync(delivery.Id);

        // Assert
        Assert.NotNull(details);
        Assert.Equal(delivery.Id.Value, details.Id);
        Assert.Equal(delivery.MerchantId.Value, details.MerchantId);
        Assert.Equal(nameof(DeliveryStatus.PendingReschedule), details.Status);
        Assert.Equal(nameof(Custody.Driver), details.Custody);
        Assert.Equal("Rua A", details.Address.Street);
        Assert.Equal(1.5m, details.Package.WeightKg);
        Assert.Equal(1u, details.Version);

        var attempt = Assert.Single(details.Attempts);
        Assert.Equal(1, attempt.AttemptNumber);
        Assert.Equal(nameof(FailureCategory.RecipientAbsent), attempt.FailureCategory);
        Assert.Equal("No answer at the door", attempt.Notes);
    }

    [Fact]
    public async Task Queries_WhenDeliveryDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var queries = scope.ServiceProvider.GetRequiredService<IDeliveryQueries>();

        // Act
        var details = await queries.GetByIdAsync(DeliveryId.New());

        // Assert
        Assert.Null(details);
    }

    [Fact]
    public async Task Repository_WhenTwoWritersUpdateSameDelivery_ShouldThrowDeliveryConcurrencyException()
    {
        // Arrange
        var delivery = CreateRequestedDelivery();
        await using var serviceProvider = CreateServiceProvider();
        await using (var seedScope = serviceProvider.CreateAsyncScope())
        {
            var repository = seedScope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
            await repository.AddAsync(delivery);
            await repository.SaveChangesAsync();
        }

        await using var firstScope = serviceProvider.CreateAsyncScope();
        await using var secondScope = serviceProvider.CreateAsyncScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
        var firstWriter = await firstRepository.GetByIdAsync(delivery.Id);
        var secondWriter = await secondRepository.GetByIdAsync(delivery.Id);

        var firstDriverId = DriverId.New();
        firstWriter!.AssignDriver(firstDriverId, Now);
        secondWriter!.AssignDriver(DriverId.New(), Now);
        await firstRepository.SaveChangesAsync();

        // Act
        var exception = await Assert.ThrowsAsync<DeliveryConcurrencyException>(
            () => secondRepository.SaveChangesAsync());

        // Assert
        Assert.Equal(delivery.Id, exception.DeliveryId);

        await using var verificationScope = serviceProvider.CreateAsyncScope();
        var verificationRepository = verificationScope.ServiceProvider.GetRequiredService<IDeliveryRepository>();
        var persisted = await verificationRepository.GetByIdAsync(delivery.Id);
        Assert.Equal(firstDriverId, persisted!.AssignedDriverId);
        Assert.Equal(DeliveryStatus.DriverAssigned, persisted.Status);
        Assert.Equal(2u, persisted.Version);
        Assert.Equal(3, await CountOutboxMessagesAsync(delivery.Id));
    }

    private ServiceProvider CreateServiceProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDeliveriesInfrastructure(fixture.ConnectionString);
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    private async Task<(Guid MessageId, DateTimeOffset? ProcessedAt, int AttemptCount, string? Error)>
        WaitForProcessedOutboxMessageAsync(DeliveryId deliveryId, string eventType)
    {
        var timeoutAt = DateTimeOffset.UtcNow.AddSeconds(10);

        while (DateTimeOffset.UtcNow < timeoutAt)
        {
            await using var dbContext = fixture.CreateDbContext();
            var connection = dbContext.Database.GetDbConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText =
                """
                SELECT id, processed_at, attempt_count, error
                FROM deliveries.outbox_messages
                WHERE aggregate_id = @aggregateId AND type = @eventType
                """;
            var aggregateId = command.CreateParameter();
            aggregateId.ParameterName = "aggregateId";
            aggregateId.Value = deliveryId.Value;
            command.Parameters.Add(aggregateId);
            var type = command.CreateParameter();
            type.ParameterName = "eventType";
            type.Value = eventType;
            command.Parameters.Add(type);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync() && !reader.IsDBNull(1))
            {
                return (
                    reader.GetGuid(0),
                    reader.GetFieldValue<DateTimeOffset>(1),
                    reader.GetInt32(2),
                    reader.IsDBNull(3) ? null : reader.GetString(3));
            }

            await Task.Delay(50);
        }

        throw new TimeoutException("The outbox message was not marked as processed in time.");
    }

    private async Task<long> CountOutboxMessagesAsync(DeliveryId deliveryId)
    {
        await using var dbContext = fixture.CreateDbContext();
        var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT COUNT(*)
            FROM deliveries.outbox_messages
            WHERE aggregate_id = @aggregateId
            """;
        var aggregateId = command.CreateParameter();
        aggregateId.ParameterName = "aggregateId";
        aggregateId.Value = deliveryId.Value;
        command.Parameters.Add(aggregateId);

        return (long)(await command.ExecuteScalarAsync())!;
    }

    private static Delivery CreateDeliveryWithFailedAttempt()
    {
        var delivery = CreateRequestedDelivery();
        var driverId = DriverId.New();
        delivery.AssignDriver(driverId, Now.AddMinutes(1));
        delivery.StartDispatchToPickup(Now.AddMinutes(2));
        delivery.ConfirmArrivalAtPickup(Now.AddMinutes(3));
        delivery.ConfirmPickup(driverId, Now.AddMinutes(4));
        delivery.RecordFailedAttempt(
            new FailureReason(FailureCategory.RecipientAbsent, "Recipient absent"),
            Now.AddMinutes(5),
            "No answer at the door");
        return delivery;
    }

    private static Delivery CreateRequestedDelivery() =>
        Delivery.Request(
            DeliveryId.New(),
            MerchantId.New(),
            CreateAddress("10"),
            new PackageInfo(1.5m, new PackageDimensions(20, 15, 10), "Parcel"),
            Now);

    private static DeliveryAddress CreateAddress(string number) =>
        new("Rua A", number, null, "Centro", "São Paulo", "SP", "01000-000");
}
